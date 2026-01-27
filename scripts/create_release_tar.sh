#!/usr/bin/env bash
set -euo pipefail

if [ $# -lt 1 ]; then
  echo "Usage: $0 <version> [output-file]"
  exit 2
fi

VERSION=$1
OUT=${2:-"mate-engine-linux-${VERSION}.tar.gz"}

echo "Creating release tarball: $OUT"
# Exclude common heavy or irrelevant folders
tar --exclude='.git' --exclude='Library' --exclude='Temp' --exclude='Logs' --exclude='.vs' -czf "$OUT" .
ls -lh "$OUT"

echo "OK"