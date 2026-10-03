#!/usr/bin/env python3
"""Offline developer reproduction of the cumulative Linux lighting host; never deploys to a user repo.

Uses an already-extracted .NET SDK and original uploaded NuGet feed. It neither
installs system packages nor downloads dependencies. Game assets are not required
for build/unit tests. The optional --window-tests flag requires a working DISPLAY.
"""
from __future__ import annotations
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET


def run(command: list[str], cwd: Path, env: dict[str, str], logs: Path, name: str) -> str:
    print(f"{name}: {' '.join(command)}", flush=True)
    result = subprocess.run(command, cwd=cwd, env=env, text=True,
                            stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    (logs / f"{name}.log").write_text(result.stdout, encoding="utf-8")
    if result.returncode:
        print(result.stdout[-12000:], file=sys.stderr)
        raise RuntimeError(f"{name} failed ({result.returncode}); see {logs}")
    return result.stdout.strip()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sdk', type=Path, required=True, help='Extracted .NET 10 SDK directory')
    parser.add_argument('--feed', type=Path, required=True, help='Extracted nuget-feed directory')
    parser.add_argument('--jobs', type=int, default=2)
    parser.add_argument('--window-tests', action='store_true')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sdk, feed = args.sdk.resolve(), args.feed.resolve()
    # Original embedded resources are compile inputs from the user's checkout;
    # the recovery overlay deliberately does not redistribute font files.
    for name in ('font1.raw', 'font2.raw'):
        resource = root / 'vendor/RecompOne/RecompOne.Runtime/Bios/Fonts' / name
        if not resource.is_file():
            raise ValueError(f'Original checkout embedded resource required: {resource.relative_to(root)}')
    dotnet = sdk / 'dotnet'
    if not dotnet.is_file() or not feed.is_dir() or not any(feed.glob('*.nupkg')):
        raise ValueError('The extracted SDK or offline NuGet feed is missing')
    if not 1 <= args.jobs <= 32:
        raise ValueError('--jobs must be between 1 and 32')
    if args.window_tests and not os.environ.get('DISPLAY'):
        raise ValueError('--window-tests requires a working X11 DISPLAY')
    build = root / 'build' / 'linux-l04'
    logs = build / 'logs'
    logs.mkdir(parents=True, exist_ok=True)
    config = ET.Element('configuration')
    sources = ET.SubElement(config, 'packageSources')
    ET.SubElement(sources, 'clear')
    ET.SubElement(sources, 'add', key='uploaded-offline-feed', value=str(feed))
    cache = ET.SubElement(config, 'config')
    ET.SubElement(cache, 'add', key='globalPackagesFolder', value=str(build / 'packages'))
    config_path = build / 'NuGet.Config'
    ET.ElementTree(config).write(config_path, encoding='utf-8', xml_declaration=True)
    env = os.environ.copy()
    env.update({'DOTNET_ROOT': str(sdk), 'DOTNET_CLI_TELEMETRY_OPTOUT': '1',
                'NUGET_CERT_REVOCATION_MODE': 'offline'})
    # Offline mode is explicit: package bytes are checked against the supplied
    # upload hashes separately; this does not claim a fresh online CRL check.
    version = run([str(dotnet), '--version'], root, env, logs, 'sdk-version')
    if not version.startswith('10.'):
        raise ValueError(f'.NET 10 required, found {version}')
    native = root / 'build' / 'native-linux'
    run(['cmake', '-S', 'native', '-B', str(native), '-DCMAKE_BUILD_TYPE=Release'], root, env, logs, 'native-configure')
    run(['cmake', '--build', str(native), '--parallel', str(args.jobs)], root, env, logs, 'native-build')
    run(['ctest', '--test-dir', str(native), '--output-on-failure'], root, env, logs, 'native-tests')
    projects = ['tools/unified-host/GranTurismo2PC.csproj',
                'tools/LinuxHostRegression/LinuxHostRegression.csproj',
                'tools/VehicleShadowRegression/VehicleShadowRegression.csproj',
                'tools/WorldCameraRegression/WorldCameraRegression.csproj']
    for ordinal, project in enumerate(projects):
        run([str(dotnet), 'restore', project, '--configfile', str(config_path),
             '--disable-parallel', '-p:NuGetAudit=false', '-p:PublishReadyToRun=false'],
            root, env, logs, f'restore-{ordinal}')
        run([str(dotnet), 'build', project, '-c', 'Release', '--no-restore',
             '-p:PublishReadyToRun=false', '-p:UseSharedCompilation=false', '-m:1'],
            root, env, logs, f'managed-build-{ordinal}')
    target = run([str(dotnet), 'msbuild', projects[0], '-nologo',
                  '-property:Configuration=Release', '-getProperty:TargetPath'], root, env, logs, 'target-path')
    command = [str(dotnet), 'run', '--project', projects[1], '-c', 'Release', '--no-build', '--no-restore']
    if args.window_tests:
        command += ['--', '--window']
    run(command, root, env, logs, 'host-regressions')
    run([str(dotnet), 'run', '--project', projects[2], '-c', 'Release',
         '--no-build', '--no-restore'], root, env, logs, 'vehicle-shadow-regressions')
    run([str(dotnet), 'run', '--project', projects[3], '-c', 'Release',
         '--no-build', '--no-restore'], root, env, logs, 'world-camera-regressions')
    result = {'sdk': version, 'gameDll': target, 'nativeTests': 'passed',
              'hostTests': 'passed', 'vehicleShadowTests': 'passed', 'worldCameraTests': 'passed', 'windowTestsRun': args.window_tests,
              'gameplayTestedByThisScript': False}
    (build / 'BUILD-RESULT.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
    return 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except (OSError, ValueError, RuntimeError) as error:
        raise SystemExit(str(error))
