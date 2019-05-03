#!/bin/sh

set -e

BRANCH="push"
REMOTE="private"

git branch $BRANCH
git filter-branch --index-filter 'git rm --cached --ignore-unmatch Encryption.cs' -f $BRANCH
git push --force -u $PRIVATE $BRANCH:master
git branch -D BRANCH