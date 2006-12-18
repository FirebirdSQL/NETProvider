PROVIDER = FirebirdSql.Data.FirebirdClient.dll
PROVIDER_TESTS = FirebirdSql.Data.FirebirdClient.UnitTests.dll

all: preprocess ${PROVIDER} ${PROVIDER_TESTS} install clean 

# TOOLS

CSC = gmcs
COPY = cp
RM = rm
MKDIR = mkdir

# Directories

BUILD_DIR = ./mono/linux
BASE_DIR=../../source/FirebirdSql
COMMON_SOURCES=${BASE_DIR}/Data/Common
CLIENTS_SOURCES=${BASE_DIR}/Data/Client
ISQL_SOURCES=${BASE_DIR}/Data/Isql
SCHEMA_SOURCES=${BASE_DIR}/Data/Schema
SERVICES_SOURCES=${BASE_DIR}/Data/Services
PROVIDER_SOURCES=${BASE_DIR}/Data/FirebirdClient
PROVIDER_TESTS_SOURCES=${BASE_DIR}/Data/UnitTest
RESOURCES=${BASE_DIR}/Resources

# RESOURCES

COMMON_RESOURCES = -resource:${RESOURCES}/isc_error_msg.resources,FirebirdSql.Data.Common.Resources.isc_error_msg.resources

TOOL_RESOURCES = -resource:${RESOURCES}/FbConnection.bmp,FirebirdSql.Data.Firebird.Resources.FbConnection.bmp -resource:${RESOURCES}/Resources/FbCommand.bmp,FirebirdSql.Data.Firebird.Resources.FbCommand.bmp -resource:${RESOURCES}/Resources/FbDataAdapter.bmp,FirebirdSql.Data.Firebird.Resources.FbDataAdapter.bmp

# DEFINES

DEFINE = -define:RELEASE -define:MONO -define:LINUX

# REFERENCES

PROVIDER_FLAGS = -reference:System.dll -reference:System.Data.dll -reference:System.Xml.dll -reference:System.Drawing.dll

PROVIDER_TESTS_FLAGS = -reference:System.dll -reference:System.Data.dll -reference:System.Xml.dll  -reference:System.Drawing.dll -reference:nunit.framework.dll -reference:${PROVIDER}

# TARGETS

FirebirdSql.Data.FirebirdClient.dll:
	$(CSC) -target:library -out:$(PROVIDER) $(PROVIDER_FLAGS) $(DEFINE) ${COMMON_RESOURCES} $(TOOL_RESOURCES) -recurse:${COMMON_SOURCES}/*.cs -recurse:${CLIENTS_SOURCES}/*.cs -recurse:${SCHEMA_SOURCES}/*.cs -recurse:${SERVICES_SOURCES}/*.cs -recurse:${ISQL_SOURCES}/*.cs -recurse:${PROVIDER_SOURCES}/*.cs

FirebirdSql.Data.FirebirdClient.UnitTests.dll:
	$(CSC) -target:library -out:$(PROVIDER_TESTS) $(PROVIDER_TESTS_FLAGS) $(DEFINE) -recurse:${PROVIDER_TESTS_SOURCES}/*.cs

preprocess:
	$(COPY) ${BASE_DIR}/*.snk .

install:
	${RM} -rf ${BUILD_DIR}
	${MKDIR} -p ${BUILD_DIR}
	$(COPY) $(PROVIDER) ${BUILD_DIR}
	$(COPY) $(PROVIDER_TESTS) ${BUILD_DIR}
	$(COPY) ${PROVIDER_TESTS_SOURCES}/App.config ${BUILD_DIR}/$(PROVIDER_TESTS).config

clean:
	${RM} FirebirdSql.Data.FirebirdClient.snk
	${RM} $(PROVIDER)
	${RM} $(PROVIDER_TESTS)
