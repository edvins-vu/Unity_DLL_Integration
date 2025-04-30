using System.Collections.Generic;

[System.Serializable]
public class Attribute
{
	public string body; 
}

[System.Serializable]
public class Fact
{
	public string id;
	public string type;
	public Attribute attributes;
}

[System.Serializable]
public class ResponseData
{
	public List<Fact> data;
}