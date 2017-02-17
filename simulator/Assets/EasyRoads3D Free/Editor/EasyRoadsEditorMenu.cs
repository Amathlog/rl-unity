using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using EasyRoads3D;
using EasyRoads3DEditor;
public class EasyRoadsEditorMenu : ScriptableObject {







[MenuItem( "GameObject/Create Other/EasyRoads3D/New EasyRoads3D Object" )]
public static void  CreateEasyRoads3DObject ()
{

RoadObjectScript[] scrpts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
if(scrpts.Length >= 1){
EditorUtility.DisplayDialog("Alert", "The Free version supports only one road editor object in the scene!\n\nPlease finalize or delete the current road object or upgrade to the full version before creating a new road object.", "Close");
Selection.activeGameObject = scrpts[0].gameObject;
return;
}
Terrain[] terrains = (Terrain[]) FindObjectsOfType(typeof(Terrain));
if(terrains.Length == 0){
EditorUtility.DisplayDialog("Alert", "No Terrain objects found! EasyRoads3D objects requires a terrain object to interact with. Please create a Terrain object first", "Close");
return;
}



if(NewEasyRoads3D.instance == null){
NewEasyRoads3D window = (NewEasyRoads3D)ScriptableObject.CreateInstance(typeof(NewEasyRoads3D));
window.ShowUtility();
}



}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Back Up/Terrain Height Data" )]
public static void  GetTerrain ()
{
if(GetEasyRoads3DObjects()){

OCQQDQCDDO.OCDOQODDOC(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Restore/Terrain Height Data" )]
public static void  SetTerrain ()
{
if(GetEasyRoads3DObjects()){

OCQQDQCDDO.OOQQOOOQQD(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Back Up/Terrain Splatmap Data" )]
public static void  OCOQOQDQQO()
{
if(GetEasyRoads3DObjects()){

ODDDCQCDCO.OCOQOQDQQO(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Restore/Terrain Splatmap Data" )]
public static void  OOQDOQOODQ ()
{
if(GetEasyRoads3DObjects()){
string path = "";
if(EditorUtility.DisplayDialog("Road Splatmap", "Would you like to merge the terrain splatmap(s) with a road splatmap?", "Yes", "No")){
path = EditorUtility.OpenFilePanel("Select png road splatmap texture", "", "png");
}


ODDDCQCDCO.ODCCQCOOCD(true, 100, 4, path, Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Back Up/Terrain Vegetation Data" )]
public static void  OCOCDQCCCO()
{
if(GetEasyRoads3DObjects()){

OCQQDQCDDO.OCOCDQCCCO(Selection.activeGameObject, null, "");
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Back Up/All Terrain Data" )]
public static void  GetAllData()
{
if(GetEasyRoads3DObjects()){

OCQQDQCDDO.OCDOQODDOC(Selection.activeGameObject);
ODDDCQCDCO.OCOQOQDQQO(Selection.activeGameObject);
OCQQDQCDDO.OCOCDQCCCO(Selection.activeGameObject, null,"");
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Restore/Terrain Vegetation Data" )]
public static void  OCODCOCDOQ()
{
if(GetEasyRoads3DObjects()){

OCQQDQCDDO.OCODCOCDOQ(Selection.activeGameObject);
}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}
}
[MenuItem( "GameObject/Create Other/EasyRoads3D/Restore/All Terrain Data" )]
public static void  RestoreAllData()
{
if(GetEasyRoads3DObjects()){

OCQQDQCDDO.OOQQOOOQQD(Selection.activeGameObject);
ODDDCQCDCO.ODCCQCOOCD(true, 100, 4, "", Selection.activeGameObject);
OCQQDQCDDO.OCODCOCDOQ(Selection.activeGameObject);

}else{
EditorUtility.DisplayDialog("Alert", "No EasyRoads3D objects found! Terrain functions cannot be accessed!", "Close");
}


}
public static bool GetEasyRoads3DObjects(){
RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
bool flag = false;
foreach (RoadObjectScript script in scripts) {
if(script.OQDDQQDOOD == null){
script.OQOOODDDDO(null, null, null);
}
flag = true;
}
return flag;
}
static private void ODQQCODDCD(RoadObjectScript target){
EditorUtility.DisplayProgressBar("Build EasyRoads3D Object - " + target.gameObject.name,"Initializing", 0);

RoadObjectScript[] scripts = (RoadObjectScript[])FindObjectsOfType(typeof(RoadObjectScript));
List<Transform> rObj = new List<Transform>();


#if UNITY_4_3

#else
//Undo.RegisterUndo(ODDDCQCDCO.terrain.terrainData, "EasyRoads3D Terrain leveling");
#endif
foreach(RoadObjectScript script in scripts) {
if(script.transform != target.transform) rObj.Add(script.transform);
}
if(target.ODODQOQO == null){
target.ODODQOQO = target.OQDDQQDOOD.OOCODCOOQC();
target.ODODQOQOInt = target.OQDDQQDOOD.ODQODCCOOO();
}
target.ODOCDCQOCC(0.5f, true, false);

List<tPoint> hitOCDODCDDQO = target.OQDDQQDOOD.ODQCDQCQQO(Vector3.zero, target.raise, target.obj, target.OOQDOOQQ, rObj, target.handleVegetation);
List<Vector3> changeArr = new List<Vector3>();
float stepsf = Mathf.Floor(hitOCDODCDDQO.Count / 10);
int steps = Mathf.RoundToInt(stepsf);
for(int i = 0; i < 10;i++){
changeArr = target.OQDDQQDOOD.OCODCCDDQQ(hitOCDODCDDQO, i * steps, steps, changeArr);
EditorUtility.DisplayProgressBar("Build EasyRoads3D Object - " + target.gameObject.name,"Updating Terrain", i * 10);
}

changeArr = target.OQDDQQDOOD.OCODCCDDQQ(hitOCDODCDDQO, 10 * steps, hitOCDODCDDQO.Count - (10 * steps), changeArr);
target.OQDDQQDOOD.OCODODCQCO(changeArr, rObj);

target.OCCQQCQCCD();
EditorUtility.ClearProgressBar();

}
private static void SetWaterScript(RoadObjectScript target){
for(int i = 0; i < target.OCCOOCQDOO.Length; i++){
if(target.OQDDQQDOOD.road.GetComponent(target.OCCOOCQDOO[i]) != null && i != target.selectedWaterScript)DestroyImmediate(target.OQDDQQDOOD.road.GetComponent(target.OCCOOCQDOO[i]));
}
if(target.OCCOOCQDOO[0] != "None Available!"  && target.OQDDQQDOOD.road.GetComponent(target.OCCOOCQDOO[target.selectedWaterScript]) == null){
#if UNITY_5_0
UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(target.OQDDQQDOOD.road, "Assets/EasyRoads3D/Editor/EasyRoadsEditorMenu.cs (460,4)", target.OCCOOCQDOO[target.selectedWaterScript]);
#else
//UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(target.OQDDQQDOOD.road, "Assets/EasyRoads3D/Editor/EasyRoadsEditorMenu.cs (463,4)", target.OCCOOCQDOO[target.selectedWaterScript]);
#endif

}
}
public static Vector3 ReadFile(string file)
{
Vector3 pos = Vector3.zero;
if(File.Exists(file)){
StreamReader streamReader = File.OpenText(file);
string line = streamReader.ReadLine();
line = line.Replace(",",".");
string[] lines = line.Split("\n"[0]);
string[] arr = lines[0].Split("|"[0]);
float.TryParse(arr[0],System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out pos.x);
float.TryParse(arr[1],System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out pos.y);
float.TryParse(arr[2],System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out pos.z);
}
return pos;
}
}
