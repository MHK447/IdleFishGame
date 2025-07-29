using UnityEngine;

public class ParallaxBackground_Type01 : MonoBehaviour
{
	[SerializeField]
	private	Transform	target;				
	[SerializeField]
	private	float		scrollAmount;		
	[SerializeField]
	private	float		moveSpeed;			
	[SerializeField]
	private	Vector3		moveDirection;		

	private void Update()
	{
		transform.position += moveDirection * moveSpeed * Time.deltaTime;

		if ( transform.position.x <= -scrollAmount )
		{
			transform.position = target.position - moveDirection * scrollAmount;
		}
	}
}

