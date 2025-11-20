using UnityEngine;
using UnityEditor;

// Custom editor class. Mostly used for hide/show fields if needed.
[CustomEditor(typeof(ShatterableGlass))]
[CanEditMultipleObjects]
public class ShatteredGlassEditor : Editor
{
    // Basicly all public ShatteredGlass's fields
    SerializedProperty Sectors;
    SerializedProperty DetailsPerSector;
    SerializedProperty SimplifyThreshold;
    SerializedProperty GlassSides;
    SerializedProperty GlassSidesMaterial;
    SerializedProperty GlassThickness;
    SerializedProperty ShatterButNotBreak;
    SerializedProperty SlightlyRotateGibs;
    SerializedProperty DestroyGibs;
    SerializedProperty AfterSeconds;
    SerializedProperty GibsOnSeparateLayer;
    SerializedProperty GibsLayer;
    SerializedProperty Force;
    SerializedProperty AdoptFragments;

    private void OnEnable()
    {
        Sectors = serializedObject.FindProperty("Sectors");
        DetailsPerSector = serializedObject.FindProperty("DetailsPerSector");
        SimplifyThreshold = serializedObject.FindProperty("SimplifyThreshold");
        GlassSides = serializedObject.FindProperty("GlassSides");
        GlassSidesMaterial = serializedObject.FindProperty("GlassSidesMaterial");
        GlassThickness = serializedObject.FindProperty("GlassThickness");
        ShatterButNotBreak = serializedObject.FindProperty("ShatterButNotBreak");
        SlightlyRotateGibs = serializedObject.FindProperty("SlightlyRotateGibs");
        DestroyGibs = serializedObject.FindProperty("DestroyGibs");
        AfterSeconds = serializedObject.FindProperty("AfterSeconds");
        GibsOnSeparateLayer = serializedObject.FindProperty("GibsOnSeparateLayer");
        GibsLayer = serializedObject.FindProperty("GibsLayer");
        Force = serializedObject.FindProperty("Force");
        AdoptFragments = serializedObject.FindProperty("AdoptFragments");
    }

    public override void OnInspectorGUI()
    {
        // This is must be here.
        serializedObject.Update();

        EditorGUILayout.IntSlider(Sectors, 1, 8, "Sectors");
        EditorGUILayout.IntSlider(DetailsPerSector, 1, 8, "Fragments per sector");
        EditorGUILayout.Slider(SimplifyThreshold, 0f, 0.5f, "Simplify threshold");
        EditorGUILayout.PropertyField(GlassSides, new GUIContent("Fragments with edges"));

        // Ask for sides Material only if sides generation enabled.
        if (GlassSides.boolValue)
            EditorGUILayout.PropertyField(GlassSidesMaterial, new GUIContent("Glass edges material"));

        EditorGUILayout.PropertyField(ShatterButNotBreak, new GUIContent("Shatter but not break"));


        if (ShatterButNotBreak.boolValue)
            // This option makes sence only for unbreakble glass.
            EditorGUILayout.PropertyField(SlightlyRotateGibs, new GUIContent("Slightly rotate fragments"));
        else
        {
            // Options for glass gibs.
            EditorGUILayout.PropertyField(DestroyGibs, new GUIContent("Destroy fragments"));

            if (DestroyGibs.boolValue)
                EditorGUILayout.PropertyField(AfterSeconds, new GUIContent("After (seconds)"));

            EditorGUILayout.PropertyField(GibsOnSeparateLayer, new GUIContent("Fragments on a separate layer"));
            if (GibsOnSeparateLayer.boolValue)
                EditorGUILayout.PropertyField(GibsLayer, new GUIContent("Fragments layer index"));
        }

        EditorGUILayout.PropertyField(Force, new GUIContent("Break force"));
        EditorGUILayout.PropertyField(GlassThickness, new GUIContent("Thickness"));
        EditorGUILayout.PropertyField(AdoptFragments, new GUIContent("Adopt fragments"));


        serializedObject.ApplyModifiedProperties();
    }
}