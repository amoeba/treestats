#!/bin/sh

set -e

BRANCH="push"
REMOTE="public"

git branch $BRANCH
git filter-branch --index-filter 'git rm --cached --ignore-unmatch Encryption.cs' -f $BRANCH
git push --force -u $REMOTE $BRANCH:master
git branch -D $BRANCH