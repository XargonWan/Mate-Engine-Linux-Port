#!/usr/bin/env bash
set -euo pipefail

OUT_DIR=dist
mkdir -p "$OUT_DIR"

# Find plugin directories under Assets/Plugins
PLUGINS_DIRS=(Assets/Plugins/*)
PACKED=()
for d in "${PLUGINS_DIRS[@]}"; do
  if [ -d "$d" ]; then
    name=$(basename "$d")
    out="$OUT_DIR/${name}.zip"
    echo "Packaging plugin $name -> $out"
    (cd "$d" && zip -r "../../$out" . >/dev/null)
    PACKED+=("$out")
  fi
done

# Create a plugins tarball
PLUGS_TAR="$OUT_DIR/plugins-$(date +%s).tar.gz"
if [ ${#PACKED[@]} -gt 0 ]; then
  echo "Creating plugins tarball $PLUGS_TAR"
  tar -czf "$PLUGS_TAR" -C "$OUT_DIR" $(basename -a "${PACKED[@]}")
  echo "$PLUGS_TAR"
else
  echo "No plugins found to package"
fi

echo "Done"