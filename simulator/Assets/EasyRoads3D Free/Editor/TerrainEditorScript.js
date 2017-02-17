@CustomEditor(EasyRoads3DTerrainID)
class TerrainEditorScript extends Editor
{
function OnSceneGUI()
{
if(Event.current.shift && RoadObjectScript.ODQQCQCOQC != null) Selection.activeGameObject = RoadObjectScript.ODQQCQCOQC.gameObject;
else RoadObjectScript.ODQQCQCOQC = null;
}
}
