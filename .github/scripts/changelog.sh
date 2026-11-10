#!/usr/bin/env bash
# changelog.sh: read and cut releases in CHANGELOG.md (Keep a Changelog). Used by release.yml.
#
#   bash .github/scripts/changelog.sh has-unreleased            exit 0 when [Unreleased] has entries
#   bash .github/scripts/changelog.sh has <version>             exit 0 when a section [<version>] exists
#   bash .github/scripts/changelog.sh cut <version> <date>      turn [Unreleased] into [<version>] - <date>, leave an empty [Unreleased]
#   bash .github/scripts/changelog.sh notes <version>           print the section's body, for the GitHub release
#
# Plain bash and awk, no GNU-only flags: it runs the same on any machine, not only on the runner.
set -euo pipefail

file="${CHANGELOG:-CHANGELOG.md}"

# Whether a section "## [<name>]" exists (a literal match: the dots in a version are not wildcards).
has_section() {
  awk -v head="## [$1]" 'index($0, head) == 1 { found = 1 } END { exit !found }' "$file"
}

# The body of one section: the lines after "## [<name>]" up to the next "## [", without leading or trailing blank lines.
section() {
  awk -v name="$1" '
    index($0, "## [") == 1 { inside = (index($0, "## [" name "]") == 1); next }
    inside { lines[++n] = $0 }
    END {
      first = 1; while (first <= n && lines[first] ~ /^[[:space:]]*$/) first++
      last = n; while (last >= first && lines[last] ~ /^[[:space:]]*$/) last--
      for (i = first; i <= last; i++) print lines[i]
    }' "$file"
}

case "${1:-}" in
  has-unreleased)
    [ -n "$(section Unreleased)" ]
    ;;
  has)
    has_section "$2"
    ;;
  notes)
    section "$2"
    ;;
  cut)
    version="$2"
    date="$3"
    if has_section "$version"; then
      echo "CHANGELOG.md already has a section for $version." >&2
      exit 1
    fi
    if [ -z "$(section Unreleased)" ]; then
      echo "CHANGELOG.md has nothing under [Unreleased]: a release with no entries says nothing to the people who install it." >&2
      exit 1
    fi
    tmp="$(mktemp)"
    awk -v version="$version" -v date="$date" '
      $0 == "## [Unreleased]" { print; print ""; print "## [" version "] - " date; next }
      { print }' "$file" > "$tmp"
    mv "$tmp" "$file"
    ;;
  *)
    sed -n '2,8p' "$0" >&2
    exit 2
    ;;
esac
