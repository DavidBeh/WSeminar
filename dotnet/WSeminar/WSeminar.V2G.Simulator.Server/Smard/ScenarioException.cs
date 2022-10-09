using System.Runtime.Serialization;

namespace WSeminar.V2G.Simulator.Server.Smard;

[Serializable]
public class ScenarioException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public ScenarioException()
    {
        
    }

    public ScenarioException(string message) : base(message)
    {
        
    }

    public ScenarioException(string message, Exception inner) : base(message, inner)
    {
    }

    protected ScenarioException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}


[Serializable]
public class ScenarioNetworkException : ScenarioException
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public ScenarioNetworkException()
    {
    }

    public ScenarioNetworkException(string message) : base(message)
    {
    }

    public ScenarioNetworkException(string message, Exception inner) : base(message, inner)
    {
    }


}