#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using Common;
	using UnityEditor;
	using UnityEngine;

	internal static class DetectorTools
	{
		public static void SetupDetectorInScene<T>() where T : KeepAliveBehaviour<T>
		{
			var component = Object.FindObjectOfType<T>();
			var detectorName = typeof(T).Name;

			if (component != null)
			{
				if (component.gameObject.name == ContainerHolder.ContainerName)
				{
					EditorUtility.DisplayDialog(detectorName + " already exists!",
						detectorName + " already exists in scene and correctly placed on object \"" + ContainerHolder.ContainerName +
						"\"", "OK");
				}
				else
				{
					var dialogResult = EditorUtility.DisplayDialogComplex(detectorName + " already exists!",
						detectorName + " already exists in scene and placed on object \"" + component.gameObject.name +
						"\". Do you wish to move it to the Game Object \"" + ContainerHolder.ContainerName +
						"\" or delete it from scene at all?", "Move", "Delete", "Cancel");
					switch (dialogResult)
					{
						case 0:
							var container = GameObject.Find(ContainerHolder.ContainerName);
							if (container == null)
							{
								container = new GameObject(ContainerHolder.ContainerName);
							}

							var newComponent = container.AddComponent<T>();
							EditorUtility.CopySerialized(component, newComponent);
							DestroyDetectorImmediate<T>(component);
							break;
						case 1:
							DestroyDetectorImmediate<T>(component);
							break;
						default:
							Debug.LogError("Unknown result from the EditorUtility.DisplayDialogComplex API!");
							break;
					}
				}
			}
			else
			{
				var container = GameObject.Find(ContainerHolder.ContainerName);
				if (container == null)
				{
					container = new GameObject(ContainerHolder.ContainerName);

					Undo.RegisterCreatedObjectUndo(container, "Create " + ContainerHolder.ContainerName);
				}

				Undo.AddComponent<T>(container);

				EditorUtility.DisplayDialog(detectorName + " added!",
					detectorName + " successfully added to the object \"" + ContainerHolder.ContainerName + "\"", "OK");
				Selection.activeGameObject = container;
			}
		}

		private static void DestroyDetectorImmediate<T>(Component component) where T: KeepAliveBehaviour<T>
		{
			if (component.transform.childCount == 0 && component.GetComponentsInChildren<Component>(true).Length <= 2)
			{
				Object.DestroyImmediate(component.gameObject);
			}
			else
			{
				Object.DestroyImmediate(component);
			}
		}
	}
}