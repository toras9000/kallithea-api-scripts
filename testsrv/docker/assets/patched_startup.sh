#!/bin/bash

# python bin
PYTHON_BIN=python3

# packages path
PYTHON_PACKAGES=$(su-exec kallithea:kallithea $PYTHON_BIN -m site --user-site)

# kallithea installation directory
KALLITEHA_INSTALL_DIR=$PYTHON_PACKAGES/kallithea

# overwrite files
if [ -n "$KALLITHEA_OVERRIDE_DIR" ] && [ -d "$KALLITHEA_OVERRIDE_DIR/kallithea" ]; then
    echo "Copy override files..."
    cp -v -RT "$KALLITHEA_OVERRIDE_DIR/kallithea"  "$KALLITEHA_INSTALL_DIR"
fi

# patch files
if [ -n "$KALLITHEA_PATCH_DIR" ] && [ -d "$KALLITHEA_PATCH_DIR" ]; then
    echo "Apply patches..."
    git -C "$KALLITEHA_INSTALL_DIR" apply --reject --whitespace=nowarn -p2 $KALLITHEA_PATCH_DIR/*
fi

# normal startup
exec bash /kallithea/startup.sh

