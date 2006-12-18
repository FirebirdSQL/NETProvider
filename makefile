LIBRARY = FirebirdSql.Data.Firebird.dll

all: $(LIBRARY) install clean 

MCS = mcs
COPY = cp
LD_FLAGS = -reference:System.dll -reference:System.Data.dll -reference:System.Drawing.dll -reference:System.Xml.dll
GDS_RESOURCES = -resource:source/Resources/GDS/isc_encodings.resources,FirebirdSql.Data.Firebird.Resources.GDS.isc_encodings.resources -resource:source/Resources/GDS/isc_error_msg.resources,FirebirdSql.Data.Firebird.Resources.GDS.isc_error_msg.resources
TOOL_RESOURCES	= -resource:source/Resources/ToolboxBitmaps/FbConnection.bmp,FirebirdSql.Data.Firebird.Resources.ToolboxBitmaps.FbConnection.bmp -resource:source/Resources/ToolboxBitmaps/FbCommand.bmp,FirebirdSql.Data.Firebird.Resources.ToolboxBitmaps.FbCommand.bmp -resource:source/Resources/ToolboxBitmaps/FbDataAdapter.bmp,FirebirdSql.Data.Firebird.Resources.ToolboxBitmaps.FbDataAdapter.bmp
RECURSE_SOURCE	= -recurse:./source/*.cs
DEFINE = -define:_DEBUG -define:_MONO

FirebirdSql.Data.Firebird.dll:
	$(MCS) -target:library -out:$(LIBRARY) $(LD_FLAGS) $(DEFINE) $(GDS_RESOURCES) $(TOOL_RESOURCES) $(RECURSE_SOURCE)

install:
	rm -rf build
	mkdir -p build
	$(COPY) $(LIBRARY) ./build

clean: 
	rm $(LIBRARY)