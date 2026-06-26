namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    private static string ToExpectationName(string name) =>
        name switch
        {
            "expectvisible" => "expectVisible",
            "expecthidden" => "expectHidden",
            "waitforvisible" => "expectVisible",
            "waitforhidden" => "expectHidden",
            "expectenabled" => "expectEnabled",
            "expectdisabled" => "expectDisabled",
            "expectvalue" => "expectValue",
            "expectvalues" => "expectValues",
            "expectattribute" => "expectAttribute",
            "expectchecked" => "expectChecked",
            "expectcount" => "expectCount",
            "tobevisible" => "expectVisible",
            "tobehidden" => "expectHidden",
            "tobeenabled" => "expectEnabled",
            "tobedisabled" => "expectDisabled",
            "tohavevalue" => "expectValue",
            "tohavevalues" => "expectValues",
            "tohaveattribute" => "expectAttribute",
            "tobechecked" => "expectChecked",
            "tohavecount" => "expectCount",
            _ => name
        };

    private static string ToNavigationExpectationName(string name) =>
        name switch
        {
            "tohaveurl" => "expectUrl",
            "tohavetitle" => "expectTitle",
            _ => name
        };
}
