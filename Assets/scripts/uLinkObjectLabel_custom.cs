// (c)2011 MuchDifferent. All Rights Reserved.

using UnityEngine;
using uLink;
using System.Collections;
using UnityEngine.UI;

[AddComponentMenu("uLink Utilities/Object Label")]
public class uLinkObjectLabel_custom : uLink.MonoBehaviour
{
	public Text prefabLabel;
	
	public bool useInitialData = false;
	
	public float minDistance = 1;
	public float maxDistance = 500;

	public Vector3 offset = new Vector3(0, 2, 0);    // Units in world space to offset; 1 unit above object by default

	public bool clampToScreen = false;  // If true, label will be visible even if object is off screen
	public float clampBorderSize = 0.05f;  // How much viewport space to leave at the borders when a label is being clamped
	
	public Color color = Color.white;
	
	public bool manualUpdate = false;
	
	public Text instantiatedLabel;

	public int playerID;

	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	{

		if (!enabled) return;

		playerID = info.networkView.owner.id;

		string text ="";

		//dont run if server
		if(uLink.Network.isServer==false)
		{
			info.networkView.initialData.TryRead<string>(out text);
		}



		//if (!info.networkView.initialData.TryRead<string>(out text))
		//{
			// TODO: log error
			//return;
		//}

		
		instantiatedLabel = (Text) Instantiate(prefabLabel, Vector3.zero, Quaternion.identity);

		instantiatedLabel.text = text;
		instantiatedLabel.material.color = color;

		instantiatedLabel.transform.SetParent(GameObject.FindGameObjectWithTag("nameDisplay").transform);
		instantiatedLabel.transform.localScale = new Vector3(1,1,1);
	}

	void OnDisable()
	{
		if (instantiatedLabel != null)
		{
			DestroyImmediate(instantiatedLabel.gameObject);
			instantiatedLabel = null;
		}
	}

	void LateUpdate()
	{
		if (manualUpdate) return;

		ManualUpdate();
	}
	
	public void ManualUpdate()	
	{
		if (instantiatedLabel == null || Camera.main == null) return;
		
		Vector3 pos;

		if (clampToScreen)
		{
			Vector3 rel = Camera.main.transform.InverseTransformPoint(transform.position);
			rel.z = Mathf.Max(rel.z, 1.0f);

			pos = Camera.main.WorldToViewportPoint(Camera.main.transform.TransformPoint(rel + offset));
			pos = new Vector3(
				Mathf.Clamp(pos.x, clampBorderSize, 1.0f - clampBorderSize),
				Mathf.Clamp(pos.y, clampBorderSize, 1.0f - clampBorderSize),
				pos.z);
		}
		else
		{
			//pos =transform.position*66.87 + offset;

			pos = new Vector2(transform.position.x*100f+offset.x, transform.position.y*100f+offset.y);

		}

		instantiatedLabel.rectTransform.anchoredPosition = pos;
		//instantiatedLabel.enabled = (pos.z >= minDistance && pos.z <= maxDistance);
	}
	
	public static void ManualUpdateAll()
	{
		uLinkObjectLabel_custom[] labels = (uLinkObjectLabel_custom[]) FindObjectsOfType(typeof(uLinkObjectLabel_custom));
		
		foreach (uLinkObjectLabel_custom label in labels)
		{
			label.ManualUpdate();
		}
	}







}
