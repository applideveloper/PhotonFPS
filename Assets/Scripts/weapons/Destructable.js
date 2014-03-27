#pragma strict
#pragma implicit
#pragma downcast

class Destructable extends MonoBehaviour
{
	function Destruct()
	{
		if(transform.childCount > 0)
		{
			var t : Transform;

			for(var i : int = 0; i < transform.childCount; i++)
			{
				t = transform.GetChild(i);
				t.parent = null;
				#if UNITY_3_5
					t.gameObject.active = true;
				#endif
				#if UNITY_4
					t.gameObject.SetActive(true);
				#endif
				if(t.renderer != null)
				{
					t.renderer.enabled = true;
				}
				
				if(t.rigidbody != null)
				{
					t.rigidbody.isKinematic = false;
				}
				
				if(t.gameObject.GetComponent("TrainingDummyPartDestructor") == null)
				{
					t.gameObject.AddComponent("TrainingDummyPartDestructor");
				}
			}
			
			if(transform.parent != null)
			{
				Destroy(transform.parent.gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}