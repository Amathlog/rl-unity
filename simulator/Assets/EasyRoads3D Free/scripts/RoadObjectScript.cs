using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;
using EasyRoads3D;

public class RoadObjectScript : MonoBehaviour {
static public string version = "";
public int objectType = 0;
public bool displayRoad = true;
public float roadWidth = 5.0f;
public float indent = 3.0f;
public float surrounding = 5.0f;
public float raise = 1.0f;
public float raiseMarkers = 0.5f;
public bool OOQDOOQQ = false;
public bool renderRoad = true;
public bool beveledRoad = false;
public bool applySplatmap = false;
public int splatmapLayer = 4;
public bool autoUpdate = true;
public float geoResolution = 5.0f;
public int roadResolution = 1;
public float tuw =  15.0f;
public int splatmapSmoothLevel;
public float opacity = 1.0f;
public int expand = 0;
public int offsetX = 0;
public int offsetY = 0;
private Material surfaceMaterial;
public float surfaceOpacity = 1.0f;
public float smoothDistance = 1.0f;
public float smoothSurDistance = 3.0f;
private bool handleInsertFlag;
public bool handleVegetation = true;
public float OOQCQCCODC = 2.0f;
public float OCCCCQCDCO = 1f;
public int materialType = 0;
String[] materialStrings;
public string uname;
public string email;
private MarkerScript[] mSc;

private bool OOOCDCDDOC;
private bool[] OQOCCODCDC = null;
private bool[] OQCCQQDDCD = null;
public string[] OOOOCCDODD;
public string[] ODODQOQO;
public int[] ODODQOQOInt;
public int ODQQOODQCD = -1;
public int ODCDOCQQQC = -1;
static public GUISkin ODCQOCODOC;
static public GUISkin OQODOOOQQQ;
public bool OODCDQCQDD = false;
private Vector3 cPos;
private Vector3 ePos;
public bool OCQQQCOCOD;
static public Texture2D ODOODQCOCO;
public int markers = 1;
public OCQCCDQCDQ OQDDQQDOOD;
private GameObject ODOQDQOO;
public bool ODQDDDCCOQ;
public bool doTerrain;
private Transform OQOCODQDCD = null;
public GameObject[] OQOCODQDCDs;
private static string OOQQQODDOO = null;
public Transform obj;
private string OOOQCDOQCD;
public static string erInit = "";
static public Transform ODQQCQCOQC;
private RoadObjectScript OQOCCDQQCQ;
public bool flyby;


private Vector3 pos;
private float fl;
private float oldfl;
private bool OQODQQDDOO;
private bool OCQODDQCDD;
private bool ODQCOQCCDD;
public Transform ODQOCCDOQQ;
public int OdQODQOD = 1;
public float OOQQQDOD = 0f;
public float OOQQQDODOffset = 0f;
public float OOQQQDODLength = 0f;
public bool ODODDDOO = false;
static public string[] ODOQDOQO;
static public string[] ODODOQQO; 
static public string[] ODODQOOQ;
public int ODQDOOQO = 0;
public string[] ODQQQQQO;  
public string[] ODODDQOO; 
public bool[] ODODQQOD; 
public int[] OOQQQOQO; 
public int ODOQOOQO = 0; 

public bool forceY = false;
public float yChange = 0f;
public float floorDepth = 2f;
public float waterLevel = 1.5f; 
public bool lockWaterLevel = true;
public float lastY = 0f;
public string distance = "0";
public string markerDisplayStr = "Hide Markers";
static public string[] objectStrings;
public string objectText = "Road";
public bool applyAnimation = false;
public float waveSize = 1.5f;
public float waveHeight = 0.15f;
public bool snapY = true;

private TextAnchor origAnchor;
public bool autoODODDQQO;
public Texture2D roadTexture;
public Texture2D roadMaterial;
public string[] ODOCQCDCDQ;
public string[] OCCOOCQDOO;
public int selectedWaterMaterial;
public int selectedWaterScript;
private bool doRestore = false;
public bool doFlyOver;
public static GameObject tracer;
public Camera goCam;
public float speed = 1f;
public float offset = 0f;
public bool camInit;
public GameObject customMesh = null;
static public bool disableFreeAlerts = true;
public bool multipleTerrains;
public bool editRestore = true;
public Material roadMaterialEdit;
static public int backupLocation = 0;
public string[] backupStrings = new string[2]{"Outside Assets folder path","Inside Assets folder path"};
public Vector3[] leftVecs = new Vector3[0];
public Vector3[] rightVecs = new Vector3[0];
public bool applyTangents = false;
public bool sosBuild = false;
public float splinePos = 0;
public float camHeight = 3;
public Vector3 splinePosV3 = Vector3.zero;
public bool blendFlag; 
public float startBlendDistance = 5;
public float endBlendDistance = 5;
public bool iOS = false;
static public string extensionPath = "";
public void OQOOODDDDO(List<ODODDQQO> arr, String[] DOODQOQO, String[] OODDQOQO){

ODQOODDCCC(transform, arr, DOODQOQO, OODDQOQO);
}
public void ODQDDDQOQC(MarkerScript markerScript){

OQOCODQDCD = markerScript.transform;



List<GameObject> tmp = new List<GameObject>();
for(int i=0;i<OQOCODQDCDs.Length;i++){
if(OQOCODQDCDs[i] != markerScript.gameObject)tmp.Add(OQOCODQDCDs[i]);
}




tmp.Add(markerScript.gameObject);
OQOCODQDCDs = tmp.ToArray();
OQOCODQDCD = markerScript.transform;

OQDDQQDOOD.ODOCCCCQQO(OQOCODQDCD, OQOCODQDCDs, markerScript.ODQOQOQOQO, markerScript.OQOCOODCQC, ODQOCCDOQQ, out markerScript.OQOCODQDCDs, out markerScript.trperc, OQOCODQDCDs);

ODCDOCQQQC = -1;
}
public void OQCCQQODQO(MarkerScript markerScript){
if(markerScript.OQOCOODCQC != markerScript.ODOOQQOO || markerScript.OQOCOODCQC != markerScript.ODOOQQOO){
OQDDQQDOOD.ODOCCCCQQO(OQOCODQDCD, OQOCODQDCDs, markerScript.ODQOQOQOQO, markerScript.OQOCOODCQC, ODQOCCDOQQ, out markerScript.OQOCODQDCDs, out markerScript.trperc, OQOCODQDCDs);
markerScript.ODQDOQOO = markerScript.ODQOQOQOQO;
markerScript.ODOOQQOO = markerScript.OQOCOODCQC;
}
if(OQOCCDQQCQ.autoUpdate) ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
}
public void ResetMaterials(MarkerScript markerScript){
if(OQDDQQDOOD != null)OQDDQQDOOD.ODOCCCCQQO(OQOCODQDCD, OQOCODQDCDs, markerScript.ODQOQOQOQO, markerScript.OQOCOODCQC, ODQOCCDOQQ, out markerScript.OQOCODQDCDs, out markerScript.trperc, OQOCODQDCDs);
}
public void OQODOQDQOC(MarkerScript markerScript){
if(markerScript.OQOCOODCQC != markerScript.ODOOQQOO){
OQDDQQDOOD.ODOCCCCQQO(OQOCODQDCD, OQOCODQDCDs, markerScript.ODQOQOQOQO, markerScript.OQOCOODCQC, ODQOCCDOQQ, out markerScript.OQOCODQDCDs, out markerScript.trperc, OQOCODQDCDs);
markerScript.ODOOQQOO = markerScript.OQOCOODCQC;
}
ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
}
private void ODDODDCQOQ(string ctrl, MarkerScript markerScript){
int i = 0;
foreach(Transform tr in markerScript.OQOCODQDCDs){
MarkerScript wsScript = (MarkerScript) tr.GetComponent<MarkerScript>();
if(ctrl == "rs") wsScript.LeftSurrounding(markerScript.rs - markerScript.ODOQQOOO, markerScript.trperc[i]);
else if(ctrl == "ls") wsScript.RightSurrounding(markerScript.ls - markerScript.DODOQQOO, markerScript.trperc[i]);
else if(ctrl == "ri") wsScript.LeftIndent(markerScript.ri - markerScript.OOQOQQOO, markerScript.trperc[i]);
else if(ctrl == "li") wsScript.RightIndent(markerScript.li - markerScript.ODODQQOO, markerScript.trperc[i]);
else if(ctrl == "rt") wsScript.LeftTilting(markerScript.rt - markerScript.ODDQODOO, markerScript.trperc[i]);
else if(ctrl == "lt") wsScript.RightTilting(markerScript.lt - markerScript.ODDOQOQQ, markerScript.trperc[i]);
else if(ctrl == "floorDepth") wsScript.FloorDepth(markerScript.floorDepth - markerScript.oldFloorDepth, markerScript.trperc[i]);
i++;
}
}
public void OQQOQQCQDO(){
if(markers > 1) ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
}
public void ODQOODDCCC(Transform tr, List<ODODDQQO> arr, String[] DOODQOQO, String[] OODDQOQO){
version = "2.5.8";
ODCQOCODOC = (GUISkin)Resources.Load("ER3DSkin", typeof(GUISkin));


ODOODQCOCO = (Texture2D)Resources.Load("ER3DLogo", typeof(Texture2D));
if(RoadObjectScript.objectStrings == null){
RoadObjectScript.objectStrings = new string[3];
RoadObjectScript.objectStrings[0] = "Road Object"; RoadObjectScript.objectStrings[1]="River Object";RoadObjectScript.objectStrings[2]="Procedural Mesh Object";
}
obj = tr;
OQDDQQDOOD = new OCQCCDQCDQ();
OQOCCDQQCQ = obj.GetComponent<RoadObjectScript>();
foreach(Transform child in obj){
if(child.name == "Markers") ODQOCCDOQQ = child;
}
RoadObjectScript[] rscrpts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
OCQCCDQCDQ.terrainList.Clear();
Terrain[] terrains = (Terrain[])FindObjectsOfType(typeof(Terrain));
foreach(Terrain terrain in terrains) {
Terrains t = new Terrains();
t.terrain = terrain;
if(!terrain.gameObject.GetComponent<EasyRoads3DTerrainID>()){
EasyRoads3DTerrainID terrainscript = (EasyRoads3DTerrainID)terrain.gameObject.AddComponent<EasyRoads3DTerrainID>();
string id = UnityEngine.Random.Range(100000000,999999999).ToString();
terrainscript.terrainid = id;
t.id = id;
}else{
t.id = terrain.gameObject.GetComponent<EasyRoads3DTerrainID>().terrainid;
}
OQDDQQDOOD.OOCCQCDQOD(t);
}
ODDDCQCDCO.OOCCQCDQOD();
if(roadMaterialEdit == null){
roadMaterialEdit = (Material)Resources.Load("materials/roadMaterialEdit", typeof(Material));
}
if(objectType == 0 && GameObject.Find(gameObject.name + "/road") == null){
GameObject road = new GameObject("road");
road.transform.parent = transform;
}

OQDDQQDOOD.ODOOCCDDDO(obj, OOQQQODDOO, OQOCCDQQCQ.roadWidth, surfaceOpacity, out OCQQQCOCOD, out indent, applyAnimation, waveSize, waveHeight);
OQDDQQDOOD.OCCCCQCDCO = OCCCCQCDCO;
OQDDQQDOOD.OOQCQCCODC = OOQCQCCODC;
OQDDQQDOOD.OdQODQOD = OdQODQOD + 1;
OQDDQQDOOD.OOQQQDOD = OOQQQDOD;
OQDDQQDOOD.OOQQQDODOffset = OOQQQDODOffset;
OQDDQQDOOD.OOQQQDODLength = OOQQQDODLength;
OQDDQQDOOD.objectType = objectType;
OQDDQQDOOD.snapY = snapY;
OQDDQQDOOD.terrainRendered = ODQDDDCCOQ;
OQDDQQDOOD.handleVegetation = handleVegetation;
OQDDQQDOOD.raise = raise;
OQDDQQDOOD.roadResolution = roadResolution;
OQDDQQDOOD.multipleTerrains = multipleTerrains;
OQDDQQDOOD.editRestore = editRestore;
OQDDQQDOOD.roadMaterialEdit = roadMaterialEdit;
OQDDQQDOOD.renderRoad = renderRoad;
OQDDQQDOOD.rscrpts = rscrpts.Length;
OQDDQQDOOD.blendFlag = blendFlag; 
OQDDQQDOOD.startBlendDistance = startBlendDistance;
OQDDQQDOOD.endBlendDistance = endBlendDistance;
if(backupLocation == 0)OCDOQDQQDQ.backupFolder = "/EasyRoads3D";
else OCDOQDQQDQ.backupFolder =  OCDOQDQQDQ.extensionPath + "/Backups";

ODODQOQO = OQDDQQDOOD.OOCODCOOQC();
ODODQOQOInt = OQDDQQDOOD.ODQODCCOOO();


if(ODQDDDCCOQ){




doRestore = true;
}


OQOQDCCDDD();

if(arr != null || ODODQOOQ == null) ODQQOCCDCC(arr, DOODQOQO, OODDQOQO);


if(doRestore) return;
}
public void UpdateBackupFolder(){
}
public void ODQDOQCQCQ(){
if(!ODODDDOO || objectType == 2){
if(OQOCCODCDC != null){
for(int i = 0; i < OQOCCODCDC.Length; i++){
OQOCCODCDC[i] = false;
OQCCQQDDCD[i] = false;
}
}
}
}

public void OODDQQCOOQ(Vector3 pos){


if(!displayRoad){
displayRoad = true;
OQDDQQDOOD.OCQOQQOCCQ(displayRoad, ODQOCCDOQQ);
}
pos.y += OQOCCDQQCQ.raiseMarkers;
if(forceY && ODOQDQOO != null){
float dist = Vector3.Distance(pos, ODOQDQOO.transform.position);
pos.y = ODOQDQOO.transform.position.y + (yChange * (dist / 100f));
}else if(forceY && markers == 0) lastY = pos.y;
GameObject go = null;
if (ODOQDQOO != null)
go = (GameObject)Instantiate (ODOQDQOO);
else {
go = Instantiate (Resources.Load ("marker", typeof(GameObject))) as GameObject;

}
Transform newnode = go.transform;
newnode.position = pos;
newnode.parent = ODQOCCDOQQ;
markers++;
string n;
if(markers < 10) n = "Marker000" + markers.ToString();
else if (markers < 100) n = "Marker00" + markers.ToString();
else n = "Marker0" + markers.ToString();
newnode.gameObject.name = n;
MarkerScript scr = newnode.GetComponent<MarkerScript>();
foreach(Transform child in go.transform)
{
if(child.name == "surface"){
scr.surface = child;
if(child.GetComponent<MeshFilter>()){
if(child.GetComponent<MeshFilter> ().sharedMesh == null)child.GetComponent<MeshFilter> ().sharedMesh = new Mesh ();
if(child.GetComponent<MeshCollider>()){
child.GetComponent<MeshCollider> ().sharedMesh = child.GetComponent<MeshFilter> ().sharedMesh;
}
}
}
}
scr.OCQQQCOCOD = false;
scr.objectScript = obj.GetComponent<RoadObjectScript>();
if(ODOQDQOO == null){
scr.waterLevel = OQOCCDQQCQ.waterLevel;
scr.floorDepth = OQOCCDQQCQ.floorDepth;
scr.ri = OQOCCDQQCQ.indent;
scr.li = OQOCCDQQCQ.indent;
scr.rs = OQOCCDQQCQ.surrounding;
scr.ls = OQOCCDQQCQ.surrounding;
scr.tension = 0.5f;
if(objectType == 1){

pos.y -= waterLevel;
newnode.position = pos;
}
}
if(objectType == 2){
#if UNITY_3_5
if(scr.surface != null)scr.surface.gameObject.active = false;
#else
if(scr.surface != null)scr.surface.gameObject.SetActive(false);
#endif
}
ODOQDQOO = newnode.gameObject;
if(markers > 1){
ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
if(materialType == 0){

OQDDQQDOOD.OOCQQDOODD(materialType);

}
}
}
public void ODOCDCQOCC(float geo, bool renderMode, bool camMode){
OQDDQQDOOD.ODQOQOOQOD.Clear();
int ii = 0;
OOCDDDDOQQ k;
foreach(Transform child  in obj)
{
if(child.name == "Markers"){
foreach(Transform marker   in child) {
MarkerScript markerScript = marker.GetComponent<MarkerScript>();
markerScript.objectScript = obj.GetComponent<RoadObjectScript>();
if(!markerScript.OCQQQCOCOD) markerScript.OCQQQCOCOD = OQDDQQDOOD.ODDCOODQQC(marker);
k  = new OOCDDDDOQQ();
k.position = marker.position;
k.num = OQDDQQDOOD.ODQOQOOQOD.Count;
k.object1 = marker;
k.object2 = markerScript.surface;
k.tension = markerScript.tension;
k.ri = markerScript.ri;
if(k.ri < 1)k.ri = 1f;
k.li =markerScript.li;
if(k.li < 1)k.li = 1f;
k.rt = markerScript.rt;
k.lt = markerScript.lt;
k.rs = markerScript.rs;
if(k.rs < 1)k.rs = 1f;
k.OQCCQDOOCD = markerScript.rs;
k.ls = markerScript.ls;
if(k.ls < 1)k.ls = 1f;
k.OCDQOOQQDD = markerScript.ls;
k.renderFlag = markerScript.bridgeObject;
k.OOCQOQOODD = markerScript.distHeights;
k.newSegment = markerScript.newSegment;
k.tunnelFlag = markerScript.tunnelFlag;
k.floorDepth = markerScript.floorDepth;
k.waterLevel = waterLevel;
k.lockWaterLevel = markerScript.lockWaterLevel;
k.sharpCorner = markerScript.sharpCorner;
k.OCOOQQOQDQ = OQDDQQDOOD;
markerScript.markerNum = ii;
markerScript.distance = "-1";
markerScript.OCDQDCCQQD = "-1";
OQDDQQDOOD.ODQOQOOQOD.Add(k);
ii++;
}
}
}
distance = "-1";

OQDDQQDOOD.OCCQOODDDC = OQOCCDQQCQ.roadWidth;

OQDDQQDOOD.OCDDDCCDOD(geo, obj, OQOCCDQQCQ.OOQDOOQQ, renderMode, camMode, objectType);
if(OQDDQQDOOD.leftVecs.Count > 0){
leftVecs = OQDDQQDOOD.leftVecs.ToArray();
rightVecs = OQDDQQDOOD.rightVecs.ToArray();
}
}
public void StartCam(){

ODOCDCQOCC(0.5f, false, true);

}
public void OQOQDCCDDD(){
int i = 0;
foreach(Transform child  in obj)
{
if(child.name == "Markers"){
i = 1;
string n;
foreach(Transform marker in child) {
if(i < 10) n = "Marker000" + i.ToString();
else if (i < 100) n = "Marker00" + i.ToString();
else n = "Marker0" + i.ToString();
marker.name = n;
ODOQDQOO = marker.gameObject;
i++;
}
}
}
markers = i - 1;

ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
}
public List<Transform> RebuildObjs(){
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
List<Transform> rObj = new List<Transform>();
foreach (RoadObjectScript script in scripts) {
if(script.transform != transform) rObj.Add(script.transform);
}
return rObj;
}
public void RestoreTerrain1(){

ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
if(OQDDQQDOOD != null) OQDDQQDOOD.OCQQQQODCD();
ODODDDOO = false;
}
public void OCCQQCQCCD(){
OQDDQQDOOD.OCCQQCQCCD(OQOCCDQQCQ.applySplatmap, OQOCCDQQCQ.splatmapSmoothLevel, OQOCCDQQCQ.renderRoad, OQOCCDQQCQ.tuw, OQOCCDQQCQ.roadResolution, OQOCCDQQCQ.raise, OQOCCDQQCQ.opacity, OQOCCDQQCQ.expand, OQOCCDQQCQ.offsetX, OQOCCDQQCQ.offsetY, OQOCCDQQCQ.beveledRoad, OQOCCDQQCQ.splatmapLayer, OQOCCDQQCQ.OdQODQOD, OOQQQDOD, OOQQQDODOffset, OOQQQDODLength);
}
public void OOCDQCCDQC(){
OQDDQQDOOD.OOCDQCCDQC(OQOCCDQQCQ.renderRoad, OQOCCDQQCQ.tuw, OQOCCDQQCQ.roadResolution, OQOCCDQQCQ.raise, OQOCCDQQCQ.beveledRoad, OQOCCDQQCQ.OdQODQOD, OOQQQDOD, OOQQQDODOffset, OOQQQDODLength);
}
public void OQQDQCDQCQ(Vector3 pos, bool doInsert){


if(!displayRoad){
displayRoad = true;
OQDDQQDOOD.OCQOQQOCCQ(displayRoad, ODQOCCDOQQ);
}

int first = -1;
int second = -1;
float dist1 = 10000;
float dist2 = 10000;
Vector3 newpos = pos;
OOCDDDDOQQ k;
OOCDDDDOQQ k1 = (OOCDDDDOQQ)OQDDQQDOOD.ODQOQOOQOD[0];
OOCDDDDOQQ k2 = (OOCDDDDOQQ)OQDDQQDOOD.ODQOQOOQOD[1];

if(doInsert){

}
OQDDQQDOOD.ODDDQDQDCQ(pos, out first, out second, out dist1, out dist2, out k1, out k2, out newpos, doInsert);
if(doInsert){


}
pos = newpos;
if(doInsert && first >= 0 && second >= 0){


if(OQOCCDQQCQ.OOQDOOQQ && second == OQDDQQDOOD.ODQOQOOQOD.Count - 1){
OODDQQCOOQ(pos);
}else{
k = (OOCDDDDOQQ)OQDDQQDOOD.ODQOQOOQOD[second];
string name = k.object1.name;
string n;
int j = second + 2;
for(int i = second; i < OQDDQQDOOD.ODQOQOOQOD.Count - 1; i++){
k = (OOCDDDDOQQ)OQDDQQDOOD.ODQOQOOQOD[i];
if(j < 10) n = "Marker000" + j.ToString();
else if (j < 100) n = "Marker00" + j.ToString();
else n = "Marker0" + j.ToString();
k.object1.name = n;
j++;
}
k = (OOCDDDDOQQ)OQDDQQDOOD.ODQOQOOQOD[first];
Transform newnode = (Transform)Instantiate(k.object1.transform, pos, k.object1.rotation);
newnode.gameObject.name = name;
newnode.parent = ODQOCCDOQQ;
newnode.SetSiblingIndex(second);
MarkerScript scr = newnode.GetComponent<MarkerScript>();
scr.OCQQQCOCOD = false;
float	totalDist = dist1 + dist2;
float perc1 = dist1 / totalDist;
float paramDif = k1.ri - k2.ri;
scr.ri = k1.ri - (paramDif * perc1);
paramDif = k1.li - k2.li;
scr.li = k1.li - (paramDif * perc1);
paramDif = k1.rt - k2.rt;
scr.rt = k1.rt - (paramDif * perc1);
paramDif = k1.lt - k2.lt;
scr.lt = k1.lt - (paramDif * perc1);
paramDif = k1.rs - k2.rs;
scr.rs = k1.rs - (paramDif * perc1);
paramDif = k1.ls - k2.ls;
scr.ls = k1.ls - (paramDif * perc1);
ODOCDCQOCC(OQOCCDQQCQ.geoResolution, false, false);
if(materialType == 0)OQDDQQDOOD.OOCQQDOODD(materialType);
#if UNITY_3_5
if(objectType == 2) scr.surface.gameObject.active = false;
#else
if(objectType == 2) scr.surface.gameObject.SetActive(false);
#endif
}
}
OQOQDCCDDD();
}
public void ODDCDODOQD(){

DestroyImmediate(OQOCCDQQCQ.OQOCODQDCD.gameObject);
OQOCODQDCD = null;
OQOQDCCDDD();
}
public void OOOQOCOCCO(){
}

public List<SideObjectParams> OOCOCDOQCQ(){
		return null;
}
public void OCCCDDQQDO(){
}
public void ODQQOCCDCC(List<ODODDQQO> arr, String[] DOODQOQO, String[] OODDQOQO){
}
public void SetMultipleTerrains(bool flag){
RoadObjectScript[] scrpts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
foreach(RoadObjectScript scr in scrpts){
scr.multipleTerrains = flag;
if(scr.OQDDQQDOOD != null)scr.OQDDQQDOOD.multipleTerrains = flag;
}
}
public bool CheckWaterHeights(){
if(ODDDCQCDCO.terrain == null) return false;
bool flag = true;

float y = ODDDCQCDCO.terrain.transform.position.y;
foreach(Transform child  in obj) {
if(child.name == "Markers"){
foreach(Transform marker  in child) {

if(marker.position.y - y <= 0.1f) flag = false;
}
}
}
return flag;
}
}
