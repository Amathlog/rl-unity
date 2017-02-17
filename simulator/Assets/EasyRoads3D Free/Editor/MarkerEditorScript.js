import EasyRoads3D;
@CustomEditor(MarkerScript)
@CanEditMultipleObjects
class MarkerEditorScript extends Editor
{
var oldPos : Vector3;
var pos : Vector3;
var ODCQOCODOC : GUISkin;
var OQODOOOQQQ : GUISkin;
var showGui : int;
var OCQODDQCDD : boolean;
var count:int = 0;
function OnEnable(){
if(target.objectScript == null) target.SetObjectScript();
else target.GetMarkerCount();
}
function OnInspectorGUI()
{


if(target.objectScript.OQOCODQDCDs == null)target.objectScript.OQOCODQDCDs = new GameObject[0];
showGui = EasyRoadsGUIMenu(false, false, target.objectScript);
if(showGui != -1 && !target.objectScript.ODODDQOO) Selection.activeGameObject =  target.transform.parent.parent.gameObject;
else if(target.objectScript.OQOCODQDCDs.length <= 1  && !target.objectScript.ODODDDOO) ERMarkerGUI(target);
else  if(target.objectScript.OQOCODQDCDs.length == 2 && !target.objectScript.ODODDDOO) MSMarkerGUI(target);
else if(target.objectScript.ODODDDOO)TRMarkerGUI(target);


}
function OnSceneGUI() {
if(target.objectScript.OQDDQQDOOD == null || target.objectScript.erInit == "") Selection.activeGameObject =  target.transform.parent.parent.gameObject;
else MarkerOnScene(target);
}
function EasyRoadsGUIMenu(flag : boolean, senderIsMain : boolean,  nRoadScript : RoadObjectScript) : int {
if((target.objectScript.OQOCCODCDC == null || target.objectScript.OQCCQQDDCD == null || target.objectScript.OQOCCDQQCQ == null) && target.objectScript.OQDDQQDOOD){
target.objectScript.OQOCCODCDC = new boolean[5];
target.objectScript.OQCCQQDDCD = new boolean[5];
target.objectScript.OQOCCDQQCQ = nRoadScript;

target.objectScript.OOOOCCDODD = target.objectScript.OQDDQQDOOD.OOQCOOCCQO();
target.objectScript.ODODQOQO = target.objectScript.OQDDQQDOOD.OOCODCOOQC();
target.objectScript.ODODQOQOInt = target.objectScript.OQDDQQDOOD.ODQODCCOOO();
}else if(target.objectScript.OQDDQQDOOD == null) return;

if(target.objectScript.ODCQOCODOC == null){
target.objectScript.ODCQOCODOC = Resources.Load("ER3DSkin", GUISkin);
target.objectScript.ODOODQCOCO = Resources.Load("ER3DLogo", Texture2D);
}
if(!flag) target.objectScript.ODQDOQCQCQ();
GUI.skin = target.objectScript.ODCQOCODOC;
EditorGUILayout.Space();
EditorGUILayout.BeginHorizontal ();
GUILayout.FlexibleSpace();
target.objectScript.OQOCCODCDC[0] = GUILayout.Toggle(target.objectScript.OQOCCODCDC[0] ,new GUIContent("", " Add road markers. "),"AddMarkers",GUILayout.Width(40), GUILayout.Height(22));
if(target.objectScript.OQOCCODCDC[0] == true && target.objectScript.OQCCQQDDCD[0] == false) {
target.objectScript.ODQDOQCQCQ();
target.objectScript.OQOCCODCDC[0] = true;  target.objectScript.OQCCQQDDCD[0] = true;
Selection.activeGameObject =  target.transform.parent.parent.gameObject;
}
target.objectScript.OQOCCODCDC[1]  = GUILayout.Toggle(target.objectScript.OQOCCODCDC[1] ,new GUIContent("", " Insert road markers. "),"insertMarkers",GUILayout.Width(40),GUILayout.Height(22));
if(target.objectScript.OQOCCODCDC[1] == true && target.objectScript.OQCCQQDDCD[1] == false) {
target.objectScript.ODQDOQCQCQ();
target.objectScript.OQOCCODCDC[1] = true;  target.objectScript.OQCCQQDDCD[1] = true;
Selection.activeGameObject =  target.transform.parent.parent.gameObject;
}
target.objectScript.OQOCCODCDC[2]  = GUILayout.Toggle(target.objectScript.OQOCCODCDC[2] ,new GUIContent("", " Process the terrain and create road geometry. "),"terrain",GUILayout.Width(40),GUILayout.Height(22));

if(target.objectScript.OQOCCODCDC[2] == true && target.objectScript.OQCCQQDDCD[2] == false) {
if(target.objectScript.markers < 2){
EditorUtility.DisplayDialog("Alert", "A minimum of 2 road markers is required before the terrain can be leveled!", "Close");
target.objectScript.OQOCCODCDC[2] = false;
}else{
target.objectScript.OQOCCODCDC[2] = false;
Selection.activeGameObject =  target.transform.parent.parent.gameObject;





}
}
if(target.objectScript.OQOCCODCDC[2] == false && target.objectScript.OQCCQQDDCD[2] == true){

target.objectScript.OQCCQQDDCD[2] = false;
Selection.activeGameObject =  target.transform.parent.parent.gameObject;
}
target.objectScript.OQOCCODCDC[3]  = GUILayout.Toggle(target.objectScript.OQOCCODCDC[3] ,new GUIContent("", " General settings. "),"settings",GUILayout.Width(40),GUILayout.Height(22));
if(target.objectScript.OQOCCODCDC[3] == true && target.objectScript.OQCCQQDDCD[3] == false) {
target.objectScript.ODQDOQCQCQ();
target.objectScript.OQOCCODCDC[3] = true;  target.objectScript.OQCCQQDDCD[3] = true;
Selection.activeGameObject =  target.transform.parent.parent.gameObject;
}
target.objectScript.OQOCCODCDC[4]  = GUILayout.Toggle(target.objectScript.OQOCCODCDC[4] ,new GUIContent("", "Version and Purchase Info"),"info",GUILayout.Width(40),GUILayout.Height(22));
if(target.objectScript.OQOCCODCDC[4] == true && target.objectScript.OQCCQQDDCD[4] == false) {
target.objectScript.ODQDOQCQCQ();
target.objectScript.OQOCCODCDC[4] = true;  target.objectScript.OQCCQQDDCD[4] = true;
Selection.activeGameObject =  target.transform.parent.parent.gameObject;
}
GUILayout.FlexibleSpace();
EditorGUILayout.EndHorizontal();
GUI.skin = null;
target.objectScript.ODQQOODQCD = -1;
for(var i : int  = 0; i < 5; i++){
if(target.objectScript.OQOCCODCDC[i]){
target.objectScript.ODQQOODQCD = i;
target.objectScript.ODCDOCQQQC = i;
}
}
if(target.objectScript.ODQQOODQCD == -1) target.objectScript.ODQDOQCQCQ();
var markerMenuDisplay : int = 1;
if(target.objectScript.ODQQOODQCD == 0 || target.objectScript.ODQQOODQCD == 1) markerMenuDisplay = 0;
else if(target.objectScript.ODQQOODQCD == 2 || target.objectScript.ODQQOODQCD == 3 || target.objectScript.ODQQOODQCD == 4) markerMenuDisplay = 0;
if(target.objectScript.ODQDDDCCOQ && !target.objectScript.OQOCCODCDC[2] && !target.objectScript.ODODDQOO){
target.objectScript.ODQDDDCCOQ = false;
if(target.objectScript.objectType != 2)target.objectScript.OCQQQQODCD();
if(target.objectScript.objectType == 2 && target.objectScript.ODQDDDCCOQ){
Debug.Log("restore");
target.objectScript.OQDDQQDOOD.OCCCOCCOOC(target.transform, true);
}
}
GUI.skin.box.alignment = TextAnchor.UpperLeft;
if(target.objectScript.ODQQOODQCD >= 0 && target.objectScript.ODQQOODQCD != 4){
if(target.objectScript.OOOOCCDODD == null || target.objectScript.OOOOCCDODD.Length == 0){

target.objectScript.OOOOCCDODD = target.objectScript.OQDDQQDOOD.OOQCOOCCQO();
target.objectScript.ODODQOQO = target.objectScript.OQDDQQDOOD.OOCODCOOQC();
target.objectScript.ODODQOQOInt = target.objectScript.OQDDQQDOOD.ODQODCCOOO();
}
EditorGUILayout.BeginHorizontal();
GUILayout.Box(target.objectScript.OOOOCCDODD[target.objectScript.ODQQOODQCD], GUILayout.MinWidth(253), GUILayout.MaxWidth(1500), GUILayout.Height(50));
EditorGUILayout.EndHorizontal();
EditorGUILayout.Space();
}
return target.objectScript.ODQQOODQCD;
}
function ERMarkerGUI( markerScript : MarkerScript){
EditorGUILayout.Space();
EditorGUILayout.BeginVertical("Box");
EditorGUILayout.LabelField(" Marker: " + (target.markerNum + 1).ToString(), EditorStyles.boldLabel);
EditorGUILayout.EndVertical();
EditorGUILayout.Space();
if(target.distance == "-1" && target.objectScript.OQDDQQDOOD != null){
var arr = target.objectScript.OQDDQQDOOD.ODCOCCOQQO(target.markerNum);
target.distance = arr[0];
target.ODQOQCQDQO = arr[1];
target.OCDQDCCQQD = arr[2];
}
GUILayout.Label("   Total Distance to Marker: " + target.distance.ToString() + " km");
GUILayout.Label("   Segment Distance to Marker: " + target.ODQOQCQDQO.ToString() + " km");
GUILayout.Label("   Marker Distance: " + target.OCDQDCCQQD.ToString() + " m");
EditorGUILayout.Space();
EditorGUILayout.BeginVertical("Box");
EditorGUILayout.LabelField(" Marker Settings", EditorStyles.boldLabel);
EditorGUILayout.EndVertical();
var oldss : boolean = markerScript.ODQOQOQOQO;
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Soft Selection", "When selected, the settings of other road markers within the selected distance will change according their distance to this marker."),  GUILayout.Width(105));
GUI.SetNextControlName ("ODQOQOQOQO");
markerScript.ODQOQOQOQO = EditorGUILayout.Toggle (markerScript.ODQOQOQOQO);
EditorGUILayout.EndHorizontal();
if(markerScript.ODQOQOQOQO){
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("         Distance", "The soft selection distance within other markers should change too."),  GUILayout.Width(105));
markerScript.OQOCOODCQC = EditorGUILayout.Slider(markerScript.OQOCOODCQC, 0, 500);
EditorGUILayout.EndHorizontal();
EditorGUILayout.Space();
}
if(oldss != markerScript.OQOCOODCQC) target.objectScript.ResetMaterials(markerScript);
GUI.enabled = false;
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Left Indent", "The distance from the left side of the road to the part of the terrain levelled at the same height as the road") ,  GUILayout.Width(105));
GUI.SetNextControlName ("ri");
oldfl = markerScript.ri;
markerScript.ri = EditorGUILayout.Slider(markerScript.ri, target.objectScript.indent, 100);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.ri){
target.objectScript.ODDODDCQOQ("ri", markerScript);
markerScript.OOQOQQOO = markerScript.ri;
}
GUI.enabled = true;
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Right Indent", "The distance from the right side of the road to the part of the terrain levelled at the same height as the road") ,  GUILayout.Width(105));
oldfl = markerScript.li;
markerScript.li =  EditorGUILayout.Slider(markerScript.li, target.objectScript.indent, 100);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.li){
target.objectScript.ODDODDCQOQ("li", markerScript);
markerScript.ODODQQOO = markerScript.li;
}
GUI.enabled = false;
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Left Surrounding", "This represents the distance over which the terrain will be gradually leveled on the left side of the road to the original terrain height"),  GUILayout.Width(105));
oldfl = markerScript.rs;
GUI.SetNextControlName ("rs");
markerScript.rs = EditorGUILayout.Slider(markerScript.rs,  target.objectScript.indent, 100);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.rs){
target.objectScript.ODDODDCQOQ("rs", markerScript);
markerScript.ODOQQOOO = markerScript.rs;
}
GUI.enabled = true;
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Right Surrounding", "This represents the distance over which the terrain will be gradually leveled on the right side of the road to the original terrain height"),  GUILayout.Width(105));
oldfl = markerScript.ls;
markerScript.ls = EditorGUILayout.Slider(markerScript.ls,  target.objectScript.indent, 100);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.ls){
target.objectScript.ODDODDCQOQ("ls", markerScript);
markerScript.DODOQQOO = markerScript.ls;
}
if(target.objectScript.objectType == 0){
GUI.enabled = false;
EditorGUILayout.BeginHorizontal();
oldfl = markerScript.rt;
GUILayout.Label(new GUIContent("    Left Tilting", "Use this setting to tilt the road on the left side (m)."),  GUILayout.Width(105));
markerScript.qt = EditorGUILayout.Slider(markerScript.qt, 0, target.objectScript.roadWidth * 0.5f);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.rt){
target.objectScript.ODDODDCQOQ("rt", markerScript);
markerScript.ODDQODOO = markerScript.rt;
}
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Right Tilting", "Use this setting to tilt the road on the right side (cm)."),  GUILayout.Width(105));
oldfl = markerScript.lt;
markerScript.lt = EditorGUILayout.Slider(markerScript.lt, 0, target.objectScript.roadWidth * 0.5f);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.lt){
target.objectScript.ODDODDCQOQ("lt", markerScript);
markerScript.ODDOQOQQ = markerScript.lt;
}
GUI.enabled = true;
if(target.markerInt < 2){
GUILayout.Label(new GUIContent("    Bridge Objects are disabled for the first two markers!", ""));
}else{
GUI.enabled = false;
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Bridge Object", "When selected this road segment will be treated as a bridge segment."),  GUILayout.Width(105));
GUI.SetNextControlName ("bridgeObject");
markerScript.bridgeObject = EditorGUILayout.Toggle (markerScript.bridgeObject);
EditorGUILayout.EndHorizontal();
EditorGUILayout.BeginHorizontal();
if(markerScript.bridgeObject){
GUILayout.Label(new GUIContent("      Distribute Heights", "When selected the terrain, the terrain will be gradually leveled between the height of this road segment and the current terrain height of the inner bridge segment."),  GUILayout.Width(135));
GUI.SetNextControlName ("distHeights");
markerScript.distHeights = EditorGUILayout.Toggle (markerScript.distHeights);
}
EditorGUILayout.EndHorizontal();
}





GUI.enabled = true;
}else{
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Floor Depth", "Use this setting to change the floor depth for this marker."),  GUILayout.Width(105));
oldfl = markerScript.floorDepth;
markerScript.floorDepth = EditorGUILayout.Slider(markerScript.floorDepth, 0, 50);
EditorGUILayout.EndHorizontal();
if(oldfl != markerScript.floorDepth){
target.objectScript.ODDODDCQOQ("floorDepth", markerScript);
markerScript.oldFloorDepth = markerScript.floorDepth;
}
}
EditorGUILayout.Space();
GUI.enabled = false;
if(target.objectScript.objectType == 0){
EditorGUILayout.BeginHorizontal();
GUILayout.Label(new GUIContent("    Start New LOD Segment", "Use this to split the road or river object in segments to use in LOD system."),  GUILayout.Width(170));
markerScript.newSegment = EditorGUILayout.Toggle (markerScript.newSegment);
EditorGUILayout.EndHorizontal();
}
GUI.enabled = true;
EditorGUILayout.Space();
if(!markerScript.autoUpdate){
EditorGUILayout.BeginHorizontal();
GUILayout.FlexibleSpace();
if(GUILayout.Button ("Refresh Surface", GUILayout.Width(225))){
target.objectScript.OQQOQQCQDO();
}
GUILayout.FlexibleSpace();
EditorGUILayout.EndHorizontal();
}
if (GUI.changed && !target.objectScript.OQODQQDDOO){
target.objectScript.OQODQQDDOO = true;
}else if(target.objectScript.OQODQQDDOO){
target.objectScript.OQCCQQODQO(markerScript);
target.objectScript.OQODQQDDOO = false;
SceneView.lastActiveSceneView.Repaint();
}
oldfl = markerScript.rs;
}
function MSMarkerGUI( markerScript : MarkerScript){
EditorGUILayout.Space();
EditorGUILayout.BeginHorizontal();
GUILayout.FlexibleSpace();
if(GUILayout.Button (new GUIContent(" Align XYZ", "Click to distribute the x, y and z values of all markers in between the selected markers in a line between the selected markers."), GUILayout.Width(150))){
Undo.RegisterUndo(target.transform.parent.GetComponentsInChildren(typeof(Transform)), "Marker align");
target.objectScript.OQDDQQDOOD.OQDDCOQQCD(target.objectScript.OQOCODQDCDs, 0);
target.objectScript.OQQOQQCQDO();
}
GUILayout.FlexibleSpace();
EditorGUILayout.EndHorizontal();
EditorGUILayout.BeginHorizontal();
GUILayout.FlexibleSpace();
if(GUILayout.Button (new GUIContent(" Align XZ", "Click to distribute the x and z values of all markers in between the selected markers in a line between the selected markers."), GUILayout.Width(150))){
Undo.RegisterUndo(target.transform.parent.GetComponentsInChildren(typeof(Transform)), "Marker align");
target.objectScript.OQDDQQDOOD.OQDDCOQQCD(target.objectScript.OQOCODQDCDs, 1);
target.objectScript.OQQOQQCQDO();
}
GUILayout.FlexibleSpace();
EditorGUILayout.EndHorizontal();
EditorGUILayout.BeginHorizontal();
GUILayout.FlexibleSpace();
if(GUILayout.Button (new GUIContent(" Align XZ  Snap Y", "Click to distribute the x and z values of all markers in between the selected markers in a line between the selected markers and snap the y value to the terrain height at the new position."), GUILayout.Width(150))){
Undo.RegisterUndo(target.transform.parent.GetComponentsInChildren(typeof(Transform)), "Marker align");
target.objectScript.OQDDQQDOOD.OQDDCOQQCD(target.objectScript.OQOCODQDCDs, 2);
target.objectScript.OQQOQQCQDO();
}
GUILayout.FlexibleSpace();
EditorGUILayout.EndHorizontal();
EditorGUILayout.BeginHorizontal();
GUILayout.FlexibleSpace();
if(GUILayout.Button (new GUIContent(" Average Heights ", "Click to distribute the heights all markers in between the selected markers."), GUILayout.Width(150))){
Undo.RegisterUndo(target.transform.parent.GetComponentsInChildren(typeof(Transform)), "Marker align");
target.objectScript.OQDDQQDOOD.OQDDCOQQCD(target.objectScript.OQOCODQDCDs, 3);
target.objectScript.OQQOQQCQDO();
}
GUILayout.FlexibleSpace();
EditorGUILayout.EndHorizontal();
EditorGUILayout.Space();
EditorGUILayout.Space();
}
function TRMarkerGUI(markerScript : MarkerScript){
EditorGUILayout.Space();
}
function MarkerOnScene(markerScript : MarkerScript){
var cEvent : Event = Event.current;

if(!target.objectScript.ODODDDOO || target.objectScript.objectType == 2){
if(cEvent.shift && (target.objectScript.ODCDOCQQQC == 0 || target.objectScript.ODCDOCQQQC == 1)) {
Selection.activeGameObject =  markerScript.transform.parent.parent.gameObject;
}else if(cEvent.shift && target.objectScript.OQOCODQDCD != target.transform){
target.objectScript.ODQDDDQOQC(markerScript);
Selection.objects = target.objectScript.OQOCODQDCDs;
}else if(target.objectScript.OQOCODQDCD != target.transform && count == 0){
if(!target.InSelected()){
target.objectScript.OQOCODQDCDs = new GameObject[0];
target.objectScript.ODQDDDQOQC(markerScript);
Selection.objects = target.objectScript.OQOCODQDCDs;


count++;
}

}else{

if(cEvent.control && !cEvent.alt)target.snapMarker = true;
else target.snapMarker = false;

pos = markerScript.oldPos;
if(pos  != oldPos && !markerScript.changed){
oldPos = pos;
if(!cEvent.shift){
target.objectScript.OQODOQDQOC(markerScript);
}
}
}
if(cEvent.shift && markerScript.changed){
OCQODDQCDD = true;
}
markerScript.changed = false;
if(!cEvent.shift && OCQODDQCDD){
target.objectScript.OQODOQDQOC(markerScript);
OCQODDQCDD = false;
}
}

}
}
