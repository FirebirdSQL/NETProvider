PROVIDER = FirebirdSql.Data.Firebird.dll
PROVIDER_TESTS = FirebirdSql.Data.Firebird.UnitTest.dll

all: preprocess ${PROVIDER} ${PROVIDER_TESTS} install clean 

# TOOLS

CSC = mcs
COPY = cp
RM = rm
MKDIR = mkdir

# Directories

BUILD_DIR = ./mono-linux
COMMON_SOURCES=../../source/FirebirdSql.Data.Common
EMBEDDED_SOURCES=../../source/FirebirdSql.Data.Embedded
GDS_SOURCES=../../source/FirebirdSql.Data.Gds
PROVIDER_SOURCES=../../source/FirebirdSql.Data.Firebird
PROVIDER_TESTS_SOURCES=../../source/FirebirdSql.Data.Firebird.UnitTest

# RESOURCES

COMMON_RESOURCES = -resource:${COMMON_SOURCES}/Resources/isc_error_msg.resources,FirebirdSql.Data.Common.Resources.isc_error_msg.resources

TOOL_RESOURCES = -resource:${PROVIDER_SOURCES}/Resources/FbConnection.bmp,FirebirdSql.Data.Firebird.Resources.FbConnection.bmp -resource:${PROVIDER_SOURCES}/Resources/FbCommand.bmp,FirebirdSql.Data.Firebird.Resources.FbCommand.bmp -resource:${PROVIDER_SOURCES}/Resources/FbDataAdapter.bmp,FirebirdSql.Data.Firebird.Resources.FbDataAdapter.bmp

# DEFINES

DEFINE = -define:RELEASE -define:MONO -define:LINUX -define:SINGLE_DLL

# REFERENCES

PROVIDER_FLAGS = -reference:System.dll -reference:System.Data.dll -reference:System.Xml.dll -reference:System.Drawing.dll

PROVIDER_TESTS_FLAGS = -reference:System.dll -reference:System.Data.dll -reference:System.Xml.dll  -reference:System.Drawing.dll -reference:nunit.framework.dll -reference:${PROVIDER}

# TARGETS

FirebirdSql.Data.Firebird.dll:
	$(CSC) -target:library -out:$(PROVIDER) $(PROVIDER_FLAGS) $(DEFINE) ${COMMON_RESOURCES} $(TOOL_RESOURCES) -recurse:${COMMON_SOURCES}/*.cs -recurse:${EMBEDDED_SOURCES}/*.cs -recurse:${GDS_SOURCES}/*.cs -recurse:${PROVIDER_SOURCES}/DbSchema/*.cs -recurse:${PROVIDER_SOURCES}/Services/*.cs -recurse:${PROVIDER_SOURCES}/Isql/*.cs ${PROVIDER_SOURCES}/*.cs

FirebirdSql.Data.Firebird.UnitTest.dll:
	$(CSC) -target:library -out:$(PROVIDER_TESTS) $(PROVIDER_TESTS_FLAGS) $(DEFINE) -recurse:${PROVIDER_TESTS_SOURCES}/*.cs

preprocess:
	$(COPY) $(PROVIDER_SOURCES)/*.snk .

install:
	${RM} -rf ${BUILD_DIR}
	${MKDIR} -p ${BUILD_DIR}
	$(COPY) $(PROVIDER) ${BUILD_DIR}
	$(COPY) $(PROVIDER_TESTS) ${BUILD_DIR}
	$(COPY) ${PROVIDER_TESTS_SOURCES}/App.config ${BUILD_DIR}/$(PROVIDER_TESTS).config

clean:
	${RM} FirebirdSql.Data.Firebird.snk
	${RM} $(PROVIDER)
	${RM} $(PROVIDER_TESTS)
