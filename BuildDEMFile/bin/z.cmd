set SRC=c:\Users\puf\Programmierung\Gis\Garmin\DEM-Test\Garmin-DEM-Build\BuildDEMFile\bin
set SRC=p:\Programmierung\!Privat\Gis\Garmin\DEM-Test\Garmin-DEM-Build\BuildDEMFile\bin

gmtool -i "%SRC%\70260025.DEM" -i "%SRC%\1881538597\70260025.LBL" -i "%SRC%\1881538597\70260025.NET" -i "%SRC%\1881538597\70260025.NOD" -i "%SRC%\1881538597\70260025.RGN" -i "%SRC%\1881538597\70260025.TRE" --join=tile -o 70260025.IMG -O

gmtool -i . --mapsource=pid:1;fid:7026;cp:1252;ov:osmmap.img;tdb:osmmap.tdb;noov;notyp;nomdx;nomdr;noinst -o . --overwrite --mapfamilyname="A, aio (70260000)" --mapseriesname="A, aio (70260000)" --description="A, aio (70260000)" --routable=1 --highestroutable=24 --maxbits4overview=18 --hasdem=1 --hasprofile=1 --copyright=*I*

del /Q "%APPDATA%\GARMIN\MapSource\TileCache\*.*"
"%ProgramFiles(x86)%/Garmin/MapSource.exe"


REM gmtool -i "%SRC%\70260025.DEM" -I 1

builddemfile --dem=70260025.DEM --hgtpath="d:\OSMData\srtm_zip" --tre="1881538597\70260025.TRE"  --dlon=0.00027761 --overwrite  --usedummydata=true --mt
builddemfile --dem=70260025.DEM --hgtpath="%OSM_DATA%\srtm_zip" --tre="1881538597\70260025.TRE"  --dlon=0.00027761 --overwrite  --usedummydata=true --mt
