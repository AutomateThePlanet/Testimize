using System;
using System.Collections.Generic;

// MCP Protocol interface (Interface Segregation)
public interface IMcpProtocolHandler
{
    object Initialize(object @params);
    object ToolsList();
    object ToolsCall(object @params);
    object GenerateTestCases(object @params);
    object GenerateHybridTestCases(object @params);
    object GeneratePairwiseTestCases(object @params);
    object ConfigureTestimizeSettings(object @params);
    object GetTestimizeSettings();
}
