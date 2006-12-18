LIBRARY = FirebirdSql.Data.Firebird.dll
NUNIT_SUITE = FirebirdSql.Data.Firebird.UnitTest.dll

all: $(LIBRARY) ${NUNIT_SUITE} install clean 

# TOOLS

CSC = mcs
COPY = cp

# REFERENCES

LD_FLAGS = -reference:System.dll -reference:System.Design.dll -reference:System.Data.dll -reference:System.Drawing.dll -reference:System.Xml.dll

# RESOURCES

GDS_RESOURCES = -resource:FirebirdSql.Data.Firebird/source/Resources/GDS/isc_error_msg.resources,FirebirdSql.Data.Firebird.Resources.GDS.isc_error_msg.resources

TOOL_RESOURCES = -resource:FirebirdSql.Data.Firebird/source/Resources/ToolboxBitmaps/FbConnection.bmp,FirebirdSql.Data.Firebird.Resources.ToolboxBitmaps.FbConnection.bmp -resource:FirebirdSql.Data.Firebird/source/Resources/ToolboxBitmaps/FbCommand.bmp,FirebirdSql.Data.Firebird.Resources.ToolboxBitmaps.FbCommand.bmp -resource:FirebirdSql.Data.Firebird/source/Resources/ToolboxBitmaps/FbDataAdapter.bmp,FirebirdSql.Data.Firebird.Resources.ToolboxBitmaps.FbDataAdapter.bmp

# SOURCES

RECURSE_SOURCE = -recurse:./FirebirdSql.Data.Firebird/source/DbSchema/*.cs -recurse:./FirebirdSql.Data.Firebird/source/GDS/*.cs -recurse:./FirebirdSql.Data.Firebird/source/Events/*.cs -recurse:./FirebirdSql.Data.Firebird/source/Services/*.cs -recurse:./FirebirdSql.Data.Firebird/source/DesignTime/ParameterCollection/*.cs
DEFINE = -define:_DEBUG -define:_MONO

RECURSE_TESTS = -recurse:./FirebirdSql.Data.Firebird.UnitTest/*.cs

# TARGETS

FirebirdSql.Data.Firebird.dll:
	$(CSC) -target:library -out:$(LIBRARY) $(LD_FLAGS) $(DEFINE) $(GDS_RESOURCES) $(TOOL_RESOURCES) $(RECURSE_SOURCE) ./FirebirdSql.Data.Firebird/source/*.cs

FirebirdSql.Data.Firebird.UnitTest.dll:
	$(CSC) -target:library -out:$(NUNIT_SUITE) $(LD_FLAGS) $(DEFINE) -reference:${LIBRARY} -reference:NUnit.Framework.dll $(RECURSE_TESTS)

install:
	rm -rf build
	mkdir -p build
	$(COPY) $(LIBRARY) ./build
	$(COPY) $(NUNIT_SUITE) ./build

clean: 
	rm $(LIBRARY)
	rm $(NUNIT_SUITE)