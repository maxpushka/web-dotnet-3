#!/bin/bash

BASE_URL='http://localhost:5006/api'

function get_token() {
    local email=$1
    local password="Mil@d1234" # Assuming the password is constant

    local token=$(curl -s $BASE_URL/account/login \
        -X POST \
        -H 'Content-Type: application/json' \
        -d "{\"email\":\"$email\",\"password\":\"$password\"}"
    )
    echo $(echo $token | jq -r .result.jwtToken)
}

function analyze_lab() {
    local access_token=$1
    local user_id=$2
  
    local result=$(curl -s $BASE_URL/analyze/new \
        -X POST \
        -H 'Content-Type: application/json' \
        -H "Authorization: Bearer ${access_token}" \
        -d "{\"UserId\":\"$user_id\",\"Name\":\"Test lab\",\"FilesContent\":{\"Program.cs\":\"Q29uc29sZS5Xcml0ZUxpbmUoIkhlbGxvIik7Cg==\"}}"
    )
    echo ${result}
}

function assert_eq() {
  local expected="$1"
  local actual="$2"
  local msg="${3-}"

  if [ "$expected" == "$actual" ]; then
    return 0
  else
    [ "${#msg}" -gt 0 ] && log_failure "$expected == $actual :: $msg" || true
    return 1
  fi
}

function log_failure() {
  printf "${RED}✖ %s${NORMAL}\n" "$@" >&2
}

function main() {
    # Authenticate Joe and upload his labs
    local joe_email='joedoe@gmail.com'
    local joe_id='c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb'
    local joe_token=$(get_token $joe_email)
    local joe_lab=$(analyze_lab $joe_token $joe_id)

    echo $joe_lab
    assert_eq $(echo $joe_lab | jq -r '.errors')         'null';
    assert_eq $(echo $joe_lab | jq -r '.result.matches') '[]';

    echo
    echo
    echo

    # Authenticate Jill and upload her labs
    local jill_email='jilldoe@gmail.com'
    local jill_id='f0dccee8-a3e1-45f8-9bb7-f7e7decebd09'
    local jill_token=$(get_token $jill_email)
    local jill_lab=$(analyze_lab $jill_token $jill_id)

    echo $jill_lab
    assert_eq $(echo $jill_lab | jq -r '.errors') 'null';
    assert_eq $(echo $jill_lab | jq -r '.result.matches.[0].duplicatedLines.[0]') 'Console.WriteLine("Hello, World");';
    assert_eq $(echo $jill_lab | jq -r '.result.matches.[0].duplicatePercentage') '100';
}
main
