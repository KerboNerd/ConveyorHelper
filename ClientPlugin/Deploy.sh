#!/usr/bin/env sh
set -eu

if [ "$#" -lt 2 ]; then
    echo "ERROR: Missing required parameters" >&2
    exit 1
fi

NAME=$1
SOURCE=${2%/}
TFM=${3:-}
PULSAR_HINT=${4:-}
PULSAR_HINT=${PULSAR_HINT%/}

DLL_PATH="$SOURCE/$NAME"
if ! [ -f "$DLL_PATH" ]; then
    echo "ERROR: Source not found: $DLL_PATH" >&2
    exit 1
fi

# Resolve Pulsar root: env -> Bin64 hint (portable) -> ~/.config/Pulsar
if [ -n "${PULSAR:-}" ]; then
    :
elif [ -n "$PULSAR_HINT" ] && { [ -d "$PULSAR_HINT/Legacy" ] || [ -d "$PULSAR_HINT/Interim" ]; }; then
    PULSAR="$PULSAR_HINT"
elif [ -d "$HOME/.config/Pulsar/Legacy" ] || [ -d "$HOME/.config/Pulsar/Interim" ]; then
    PULSAR="$HOME/.config/Pulsar"
elif [ -n "$PULSAR_HINT" ]; then
    PULSAR="$PULSAR_HINT"
else
    PULSAR="$HOME/.config/Pulsar"
fi

# Determine the destination Local plugin folder.
# Priority: explicit override -> per-framework routing.
#   net4x  (.NET Framework) -> Pulsar/Legacy/Local
#   others (.NET 5+)        -> Pulsar/Interim/Local when the Interim edition exists
if [ -n "${PULSAR_LOCAL_DIR:-}" ]; then
    PLUGIN_DIR="$PULSAR_LOCAL_DIR"
    mkdir -p "$PLUGIN_DIR"
else
    case "$TFM" in
        net4*)
            if [ ! -d "$PULSAR/Legacy" ]; then
                echo "Pulsar Legacy not installed, skipping $TFM deploy: $PULSAR/Legacy" >&2
                echo "Set PULSAR_LOCAL_DIR to your Pulsar Local folder if it is elsewhere." >&2
                exit 0
            fi
            PLUGIN_DIR="$PULSAR/Legacy/Local"
            mkdir -p "$PLUGIN_DIR"
            ;;
        *)
            if [ -d "$PULSAR/Interim" ]; then
                PLUGIN_DIR="$PULSAR/Interim/Local"
                mkdir -p "$PLUGIN_DIR"
            elif [ -d "$PULSAR/Local" ]; then
                PLUGIN_DIR="$PULSAR/Local"
            else
                echo "Pulsar Interim not installed, skipping $TFM deploy: $PULSAR/Interim" >&2
                echo "Set PULSAR_LOCAL_DIR to your Pulsar Local folder if it is elsewhere." >&2
                exit 0
            fi
            ;;
    esac
fi

echo "Copying \"$DLL_PATH\" to \"$PLUGIN_DIR/\""
if ! cp -f "$DLL_PATH" "$PLUGIN_DIR/"; then
    echo "WARNING: Could not copy \"$NAME\" — file is probably locked by a running game/Pulsar." >&2
    echo "Build succeeded; close the game and rebuild to refresh the deployed plugin." >&2
    exit 0
fi
