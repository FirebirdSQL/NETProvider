#!/bin/sh

base_dir=./source

COMMON_PATH=$base_dir/FirebirdSql.Data.Common
EMBEDDED_PATH=$base_dir/FirebirdSql.Data.Embedded
PROVIDER_PATH=$base_dir/FirebirdSql.Data.Firebird
GDS_PATH=$base_dir/FirebirdSql.Data.Gds
CLIENT_NUNIT_TESTS=$base_dir/FirebirdSql.Data.Client.UnitTest
PROVIDER_NUNIT_TESTS=$base_dir/FirebirdSql.Data.Firebird.UnitTest

SOURCES_PATH=${COMMON_PATH}:${EMBEDDED_PATH}:${PROVIDER_PATH}:${GDS_PATH}:${CLIENT_NUNIT_TESTS}:${PROVIDER_NUNIT_TESTS}

for dir in `echo ${SOURCES_PATH} | tr ":" " "`; do
    echo $dir
    for fi in `ls $dir/*.cs`;do
        dos2unix -U $fi
    done
done

dos2unix -U *.txt
dos2unix -U *.TXT
dos2unix -U *.build
dos2unix -U *.bat
dos2unix -U *.html
