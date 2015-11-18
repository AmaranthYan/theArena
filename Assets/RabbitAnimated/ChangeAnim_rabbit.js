public var currentClip = 0;

function Update () 
{	
	if(Input.GetKeyDown(KeyCode.UpArrow))
	{
		for (var state : AnimationState in GetComponent.<Animation>()) 
		{
   			state.speed += 0.2;
		}
	}
	else if	(Input.GetKeyDown(KeyCode.DownArrow))
	{
		for (var state : AnimationState in GetComponent.<Animation>()) 
		{
   			state.speed -= 0.2;
		}
	}
	
/*	if(Input.GetKeyDown(KeyCode.Mouse0))
	{
	*/		
		if(currentClip == 0)
		{
        	GetComponent.<Animation>().CrossFade("walk", 0.2);
		}
		else if(currentClip == 1)
		{
        	GetComponent.<Animation>().CrossFade("run", 0.2);
		}
		else if(currentClip == 2)
        {
       		GetComponent.<Animation>().CrossFade("lookOut", 0.2);
        }
//	}
	if(!GetComponent.<Animation>().isPlaying)
	{
		currentClip +=1;
		if(currentClip ==3)
			currentClip = 0;
	}
}