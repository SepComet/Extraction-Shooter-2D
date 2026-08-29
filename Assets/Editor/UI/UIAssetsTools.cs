using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UI.Editor
{
    public static class UIAssetsTools
    {
        public const string GeneratedNamespace = "UI";
        public const string GeneratedDirectory = "Assets/Scripts/UI/Generated";

        private const string PendingGenerationKey = "UI.Serialization.PendingGeneration";
        private static bool finalizeQueued;

        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
            "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
            "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
            "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
            "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };

        [Serializable]
        private sealed class PendingGenerationBatch
        {
            public List<PendingGeneration> entries = new List<PendingGeneration>();
            public string primaryScriptPath;
        }

        [Serializable]
        private sealed class PendingGeneration
        {
            public string rootGlobalId;
            public string prefabAssetPath;
            public string prefabRootRelativePath;
            public string scenePath;
            public string hierarchyPath;
            public string classFullName;
            public string scriptPath;
        }

        private sealed class GenerationUnit
        {
            public UISerializationRoot root;
            public string classFullName;
            public string scriptPath;
            public string source;
            public bool sourceChanged;
        }

        private sealed class Binding
        {
            public Component component;
            public UISerializationRoot nestedRoot;
            public string variableName;
            public string csharpTypeName;
        }

        [MenuItem("Assets/Create View Script", false, 2000)]
        private static void GenerateFromAssetsMenu()
        {
            GenerateWithDialog(GetSelectedRoot());
        }

        [MenuItem("Assets/Create View Script", true)]
        private static bool ValidateAssetsMenu()
        {
            return GetSelectedRoot() != null;
        }

        [MenuItem("GameObject/Create View Script", false, 30)]
        private static void GenerateFromGameObjectMenu()
        {
            GenerateWithDialog(GetSelectedRoot());
        }

        [MenuItem("GameObject/Create View Script", true)]
        private static bool ValidateGameObjectMenu()
        {
            return GetSelectedRoot() != null;
        }

        [MenuItem("CONTEXT/UISerializationRoot/Create View Script")]
        private static void GenerateFromComponentMenu(MenuCommand command)
        {
            GenerateWithDialog(command.context as UISerializationRoot);
        }

        public static void GenerateWithDialog(UISerializationRoot root)
        {
            try
            {
                Generate(root, true);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Create UI View Script", exception.Message, "OK");
            }
        }

        public static string Generate(UISerializationRoot root, bool confirmOverwrite)
        {
            if (root == null)
            {
                throw new InvalidOperationException("Select a GameObject or Prefab with UISerializationRoot.");
            }

            List<UISerializationRoot> generationRoots = CollectGenerationRoots(root);
            List<GenerationUnit> units = BuildGenerationUnits(generationRoots);
            GenerationUnit primaryUnit = units[units.Count - 1];

            List<string> changedExistingFiles = units
                .Where(unit => unit.sourceChanged && File.Exists(unit.scriptPath))
                .Select(unit => unit.scriptPath)
                .ToList();
            if (confirmOverwrite && changedExistingFiles.Count > 0)
            {
                string message = "Overwrite the following generated View scripts?\n\n" +
                                 string.Join("\n", changedExistingFiles.ToArray());
                if (!EditorUtility.DisplayDialog("Regenerate UI View", message, "Overwrite", "Cancel"))
                {
                    return null;
                }
            }

            EnsureFolder(GeneratedDirectory);
            SaveDirtyRootAssets(units);

            var pendingBatch = new PendingGenerationBatch
            {
                primaryScriptPath = primaryUnit.scriptPath
            };
            for (int i = 0; i < units.Count; i++)
            {
                pendingBatch.entries.Add(CreatePendingGeneration(units[i]));
            }
            SessionState.SetString(PendingGenerationKey, JsonUtility.ToJson(pendingBatch));

            bool anySourceChanged = false;
            for (int i = 0; i < units.Count; i++)
            {
                GenerationUnit unit = units[i];
                if (!unit.sourceChanged)
                {
                    continue;
                }

                File.WriteAllText(unit.scriptPath, unit.source, new UTF8Encoding(false));
                anySourceChanged = true;
            }

            if (anySourceChanged)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            UnityEngine.Object scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(primaryUnit.scriptPath);
            if (scriptAsset != null)
            {
                EditorGUIUtility.PingObject(scriptAsset);
            }

            if (anySourceChanged)
            {
                CompilationPipeline.RequestScriptCompilation();
            }
            else
            {
                QueuePendingGeneration();
            }

            Debug.Log("Generated " + units.Count + " UI view script(s); primary script: " + primaryUnit.scriptPath);
            return primaryUnit.scriptPath;
        }

        public static string GetViewClassName(UISerializationRoot root)
        {
            if (root == null)
            {
                return string.Empty;
            }

            string sourceName = root.gameObject.name.Replace("(Clone)", string.Empty).Trim();
            if (sourceName.EndsWith("UI", StringComparison.Ordinal) && sourceName.Length > 2)
            {
                sourceName = sourceName.Substring(0, sourceName.Length - 2);
            }

            string identifier = ToPascalIdentifier(sourceName);
            if (string.IsNullOrEmpty(identifier))
            {
                throw new InvalidOperationException("The UI root name cannot be converted to a C# class name.");
            }

            return identifier + "View";
        }

        public static bool IsValidIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || CSharpKeywords.Contains(value))
            {
                return false;
            }

            if (!char.IsLetter(value[0]) && value[0] != '_')
            {
                return false;
            }

            for (int i = 1; i < value.Length; i++)
            {
                if (!char.IsLetterOrDigit(value[i]) && value[i] != '_')
                {
                    return false;
                }
            }

            return true;
        }

        [InitializeOnLoadMethod]
        private static void InitializePendingGeneration()
        {
            QueuePendingGeneration();
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            QueuePendingGeneration();
        }

        private static void QueuePendingGeneration()
        {
            if (finalizeQueued || string.IsNullOrEmpty(SessionState.GetString(PendingGenerationKey, string.Empty)))
            {
                return;
            }

            finalizeQueued = true;
            EditorApplication.delayCall += RunQueuedPendingGeneration;
        }

        private static void RunQueuedPendingGeneration()
        {
            finalizeQueued = false;
            FinalizePendingGeneration();
        }

        private static void FinalizePendingGeneration()
        {
            string json = SessionState.GetString(PendingGenerationKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            PendingGenerationBatch batch = JsonUtility.FromJson<PendingGenerationBatch>(json);
            if (batch == null || batch.entries == null || batch.entries.Count == 0)
            {
                SessionState.EraseString(PendingGenerationKey);
                Debug.LogError("The pending UI View generation data is invalid.");
                return;
            }

            var resolvedTypes = new List<Type>(batch.entries.Count);
            for (int i = 0; i < batch.entries.Count; i++)
            {
                PendingGeneration pending = batch.entries[i];
                Type viewType = FindViewType(pending.classFullName);
                if (viewType == null)
                {
                    Debug.LogError("Generated UI view type was not found after compilation: " + pending.classFullName);
                    return;
                }

                resolvedTypes.Add(viewType);
            }

            SessionState.EraseString(PendingGenerationKey);
            UnityEngine.Object primaryTarget = null;
            for (int i = 0; i < batch.entries.Count; i++)
            {
                PendingGeneration pending = batch.entries[i];
                Type viewType = resolvedTypes[i];
                if (!string.IsNullOrEmpty(pending.prefabAssetPath))
                {
                    primaryTarget = ApplyToPrefab(pending, viewType);
                    continue;
                }

                UISerializationRoot root = ResolveSceneRoot(pending);
                if (root == null)
                {
                    throw new InvalidOperationException(
                        "Could not resolve the UI root after compilation. The script was generated at " + pending.scriptPath + ".");
                }

                ApplyViewReferences(root, viewType, true);
                EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
                primaryTarget = root.gameObject;
            }

            if (primaryTarget != null)
            {
                Selection.activeObject = primaryTarget;
                EditorGUIUtility.PingObject(primaryTarget);
            }

            Debug.Log("Attached and populated " + batch.entries.Count + " generated UI view(s).");
        }

        private static List<UISerializationRoot> CollectGenerationRoots(UISerializationRoot selectedRoot)
        {
            bool persistentContext = EditorUtility.IsPersistent(selectedRoot);
            var result = new List<UISerializationRoot>();
            var visited = new HashSet<UISerializationRoot>();
            CollectGenerationRootsRecursive(selectedRoot, persistentContext, visited, result);
            return result;
        }

        private static void CollectGenerationRootsRecursive(
            UISerializationRoot current,
            bool persistentContext,
            HashSet<UISerializationRoot> visited,
            List<UISerializationRoot> result)
        {
            current = GetCanonicalRoot(current, persistentContext);
            if (current == null || !visited.Add(current))
            {
                return;
            }

            current.RefreshItems();
            for (int i = 0; i < current.NestedViewReferences.Count; i++)
            {
                UISerializationRoot nestedRoot = current.NestedViewReferences[i].Root;
                if (nestedRoot != null)
                {
                    CollectGenerationRootsRecursive(nestedRoot, persistentContext, visited, result);
                }
            }

            result.Add(current);
        }

        private static UISerializationRoot GetCanonicalRoot(UISerializationRoot root, bool persistentContext)
        {
            if (!persistentContext || root == null)
            {
                return root;
            }

            UISerializationRoot source = PrefabUtility.GetCorrespondingObjectFromSource(root);
            return source != null ? source : root;
        }

        private static List<GenerationUnit> BuildGenerationUnits(List<UISerializationRoot> roots)
        {
            var units = new List<GenerationUnit>(roots.Count);
            var classOwners = new Dictionary<string, UISerializationRoot>(StringComparer.Ordinal);
            for (int i = 0; i < roots.Count; i++)
            {
                UISerializationRoot root = roots[i];
                root.RefreshItems();
                string className = GetViewClassName(root);
                UISerializationRoot existingOwner;
                if (classOwners.TryGetValue(className, out existingOwner) && existingOwner != root)
                {
                    throw new InvalidOperationException(
                        "Multiple serialization roots generate the same View class '" + className + "'. Rename one of the UI roots.");
                }
                classOwners[className] = root;

                List<Binding> bindings = CollectBindings(root);
                string scriptPath = GeneratedDirectory + "/" + className + ".cs";
                string source = BuildSource(className, bindings);
                units.Add(new GenerationUnit
                {
                    root = root,
                    classFullName = GeneratedNamespace + "." + className,
                    scriptPath = scriptPath,
                    source = source,
                    sourceChanged = !File.Exists(scriptPath) || File.ReadAllText(scriptPath) != source
                });
            }

            return units;
        }

        private static List<Binding> CollectBindings(UISerializationRoot root)
        {
            var bindings = new List<Binding>();
            var names = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < root.SerializationItems.Count; i++)
            {
                UISerializationItem item = root.SerializationItems[i];
                if (item == null)
                {
                    continue;
                }

                item.RefreshComponents();
                for (int j = 0; j < item.ComponentReferences.Count; j++)
                {
                    UISerializationItem.ComponentReference reference = item.ComponentReferences[j];
                    if (reference == null || !reference.GenerateReference)
                    {
                        continue;
                    }

                    Component component = reference.Component;
                    string variableName = NormalizeAndValidateVariableName(reference.VariableName, item.transform, names);
                    if (component == null)
                    {
                        throw new InvalidOperationException("A selected UI reference is missing on " + GetHierarchyPath(item.transform) + ".");
                    }
                    if (component.GetType().Namespace != null && component.GetType().Namespace.StartsWith("UnityEditor", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "Editor-only components cannot be referenced by a runtime View: " + component.GetType().FullName + ".");
                    }

                    bindings.Add(new Binding
                    {
                        component = component,
                        variableName = variableName,
                        csharpTypeName = GetCSharpTypeName(component.GetType())
                    });
                }
            }

            for (int i = 0; i < root.NestedViewReferences.Count; i++)
            {
                UISerializationRoot.NestedViewReference reference = root.NestedViewReferences[i];
                if (reference == null || !reference.GenerateReference)
                {
                    continue;
                }

                UISerializationRoot nestedRoot = reference.Root;
                if (nestedRoot == null)
                {
                    throw new InvalidOperationException("A nested UI View reference is missing below " + GetHierarchyPath(root.transform) + ".");
                }

                string variableName = NormalizeAndValidateVariableName(reference.VariableName, nestedRoot.transform, names);
                bindings.Add(new Binding
                {
                    nestedRoot = nestedRoot,
                    variableName = variableName,
                    csharpTypeName = "global::" + GeneratedNamespace + "." + GetViewClassName(nestedRoot)
                });
            }

            return bindings;
        }

        private static string NormalizeAndValidateVariableName(string value, Transform owner, HashSet<string> names)
        {
            string variableName = value == null ? string.Empty : value.Trim();
            if (!IsValidIdentifier(variableName))
            {
                throw new InvalidOperationException(
                    "Invalid C# field name '" + variableName + "' on " + GetHierarchyPath(owner) + ".");
            }
            if (!names.Add(variableName))
            {
                throw new InvalidOperationException("Duplicate UI field name '" + variableName + "'.");
            }
            if (typeof(MonoBehaviour).GetMember(variableName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length > 0)
            {
                throw new InvalidOperationException("UI field name '" + variableName + "' conflicts with a MonoBehaviour member.");
            }

            return variableName;
        }

        private static string BuildSource(string className, List<Binding> bindings)
        {
            var builder = new StringBuilder(512 + bindings.Count * 96);
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.Append("namespace ").AppendLine(GeneratedNamespace);
            builder.AppendLine("{");
            builder.AppendLine("    [DisallowMultipleComponent]");
            builder.Append("    public partial class ").Append(className).AppendLine(" : MonoBehaviour");
            builder.AppendLine("    {");

            for (int i = 0; i < bindings.Count; i++)
            {
                Binding binding = bindings[i];
                builder.Append("        [SerializeField] public ")
                    .Append(binding.csharpTypeName)
                    .Append(' ')
                    .Append(binding.variableName)
                    .AppendLine(";");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static PendingGeneration CreatePendingGeneration(GenerationUnit unit)
        {
            UISerializationRoot root = unit.root;
            bool isPrefabAsset = EditorUtility.IsPersistent(root) && PrefabUtility.IsPartOfPrefabAsset(root);
            string prefabAssetPath = isPrefabAsset ? AssetDatabase.GetAssetPath(root.gameObject) : string.Empty;
            string relativePath = string.Empty;
            if (isPrefabAsset)
            {
                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                if (prefabRoot == null)
                {
                    throw new InvalidOperationException("Could not load Prefab asset: " + prefabAssetPath);
                }

                relativePath = GetRelativePath(prefabRoot.transform, root.transform);
            }

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(root);
            return new PendingGeneration
            {
                rootGlobalId = globalObjectId.ToString(),
                prefabAssetPath = prefabAssetPath,
                prefabRootRelativePath = relativePath,
                scenePath = root.gameObject.scene.path,
                hierarchyPath = GetHierarchyPath(root.transform),
                classFullName = unit.classFullName,
                scriptPath = unit.scriptPath
            };
        }

        private static UnityEngine.Object ApplyToPrefab(PendingGeneration pending, Type viewType)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(pending.prefabAssetPath);
            try
            {
                Transform rootTransform = string.IsNullOrEmpty(pending.prefabRootRelativePath)
                    ? contents.transform
                    : contents.transform.Find(pending.prefabRootRelativePath);
                UISerializationRoot root = rootTransform == null ? null : rootTransform.GetComponent<UISerializationRoot>();
                if (root == null)
                {
                    throw new InvalidOperationException(
                        "The Prefab no longer contains the expected UISerializationRoot at '" +
                        pending.prefabRootRelativePath + "': " + pending.prefabAssetPath);
                }

                ApplyViewReferences(root, viewType, false);
                PrefabUtility.SaveAsPrefabAsset(contents, pending.prefabAssetPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            AssetDatabase.ImportAsset(pending.prefabAssetPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<GameObject>(pending.prefabAssetPath);
        }

        private static Component ApplyViewReferences(UISerializationRoot root, Type viewType, bool useUndo)
        {
            root.RefreshItems();
            List<Binding> bindings = CollectBindings(root);
            Component view = root.GetComponent(viewType);
            if (view == null)
            {
                view = useUndo ? Undo.AddComponent(root.gameObject, viewType) : root.gameObject.AddComponent(viewType);
            }
            else if (useUndo)
            {
                Undo.RecordObject(view, "Populate UI view references");
            }

            var serializedView = new SerializedObject(view);
            serializedView.UpdateIfRequiredOrScript();
            for (int i = 0; i < bindings.Count; i++)
            {
                Binding binding = bindings[i];
                SerializedProperty property = serializedView.FindProperty(binding.variableName);
                if (property == null)
                {
                    throw new InvalidOperationException("Generated field was not found: " + binding.variableName + ".");
                }

                UnityEngine.Object target = binding.component;
                if (binding.nestedRoot != null)
                {
                    string nestedViewFullName = GeneratedNamespace + "." + GetViewClassName(binding.nestedRoot);
                    Type nestedViewType = FindViewType(nestedViewFullName);
                    if (nestedViewType == null)
                    {
                        throw new InvalidOperationException("Nested UI view type was not found: " + nestedViewFullName + ".");
                    }

                    target = binding.nestedRoot.GetComponent(nestedViewType);
                    if (target == null)
                    {
                        throw new InvalidOperationException(
                            "Nested UI root '" + GetHierarchyPath(binding.nestedRoot.transform) +
                            "' does not have its generated View component '" + nestedViewFullName + "'.");
                    }
                }

                property.objectReferenceValue = target;
            }

            serializedView.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);
            if (useUndo && PrefabUtility.IsPartOfPrefabInstance(view))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            }

            return view;
        }

        private static Type FindViewType(string fullName)
        {
            return TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .FirstOrDefault(type => type.FullName == fullName);
        }

        private static UISerializationRoot ResolveSceneRoot(PendingGeneration pending)
        {
            GlobalObjectId parsed;
            if (GlobalObjectId.TryParse(pending.rootGlobalId, out parsed))
            {
                UnityEngine.Object resolved = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsed);
                UISerializationRoot root = resolved as UISerializationRoot;
                if (root != null)
                {
                    return root;
                }

                GameObject resolvedObject = resolved as GameObject;
                if (resolvedObject != null)
                {
                    root = resolvedObject.GetComponent<UISerializationRoot>();
                    if (root != null)
                    {
                        return root;
                    }
                }
            }

            UISerializationRoot[] loadedRoots = Resources.FindObjectsOfTypeAll<UISerializationRoot>();
            for (int i = 0; i < loadedRoots.Length; i++)
            {
                UISerializationRoot root = loadedRoots[i];
                if (root != null && !EditorUtility.IsPersistent(root) && root.gameObject.scene.path == pending.scenePath &&
                    GetHierarchyPath(root.transform) == pending.hierarchyPath)
                {
                    return root;
                }
            }

            return null;
        }

        private static UISerializationRoot GetSelectedRoot()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                selected = Selection.activeObject as GameObject;
            }

            return selected == null ? null : selected.GetComponent<UISerializationRoot>();
        }

        private static string GetCSharpTypeName(Type type)
        {
            if (type.IsArray)
            {
                return GetCSharpTypeName(type.GetElementType()) + "[]";
            }

            if (type.IsGenericType)
            {
                string definition = type.GetGenericTypeDefinition().FullName ?? type.Name;
                int arityMarker = definition.IndexOf('`');
                if (arityMarker >= 0)
                {
                    definition = definition.Substring(0, arityMarker);
                }

                string arguments = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName).ToArray());
                return "global::" + definition.Replace('+', '.') + "<" + arguments + ">";
            }

            return "global::" + (type.FullName ?? type.Name).Replace('+', '.');
        }

        private static void SaveDirtyRootAssets(List<GenerationUnit> units)
        {
            var savedPaths = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < units.Count; i++)
            {
                UISerializationRoot root = units[i].root;
                if (root == null || !EditorUtility.IsPersistent(root))
                {
                    continue;
                }

                string assetPath = AssetDatabase.GetAssetPath(root);
                if (string.IsNullOrEmpty(assetPath) || !savedPaths.Add(assetPath))
                {
                    continue;
                }

                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assetPath));
            }
        }

        private static string ToPascalIdentifier(string value)
        {
            var builder = new StringBuilder(value.Length + 8);
            bool capitalizeNext = true;
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (!char.IsLetterOrDigit(current) && current != '_')
                {
                    capitalizeNext = true;
                    continue;
                }

                if (builder.Length == 0 && char.IsDigit(current))
                {
                    builder.Append('_');
                }

                builder.Append(capitalizeNext ? char.ToUpperInvariant(current) : current);
                capitalizeNext = false;
            }

            return builder.ToString();
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == target)
            {
                return string.Empty;
            }

            var parts = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                parts.Push(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                throw new InvalidOperationException("The serialization root is not inside the expected Prefab asset.");
            }

            return string.Join("/", parts.ToArray());
        }

        private static string GetHierarchyPath(Transform target)
        {
            string path = target.name;
            Transform current = target.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
