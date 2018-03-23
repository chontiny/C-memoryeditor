namespace Squalr.Engine.Scripting
{
    using System;

    public interface IProxyCompiler
    {
        Byte[] CompileScript(String script);
    }
    //// End interface
}
//// End namespace