namespace CMG.Runner;

public sealed partial class CmgActionLowerer
{
    private static string ToExpectationName(string name) =>
        name switch
        {
            "expectvisible" => "expectVisible",
            "expecthidden" => "expectHidden",
            "expectnotvisible" => "expectHidden",
            "expectnothidden" => "expectVisible",
            "waitforvisible" => "expectVisible",
            "waitforhidden" => "expectHidden",
            "expectenabled" => "expectEnabled",
            "expectdisabled" => "expectDisabled",
            "expectnotenabled" => "expectDisabled",
            "expectnotdisabled" => "expectEnabled",
            "expectvalue" => "expectValue",
            "expectvalues" => "expectValues",
            "expectattribute" => "expectAttribute",
            "expectaccessiblename" => "expectAccessibleName",
            "expectrole" => "expectRole",
            "expectchecked" => "expectChecked",
            "unchecked" => "expectUnchecked",
            "expectunchecked" => "expectUnchecked",
            "expectcount" => "expectCount",
            "tobevisible" => "expectVisible",
            "tobehidden" => "expectHidden",
            "tobenotvisible" => "expectHidden",
            "tobenothidden" => "expectVisible",
            "tobeenabled" => "expectEnabled",
            "tobedisabled" => "expectDisabled",
            "tobenotenabled" => "expectDisabled",
            "tobenotdisabled" => "expectEnabled",
            "tohavevalue" => "expectValue",
            "tohavevalues" => "expectValues",
            "tohaveattribute" => "expectAttribute",
            "tohaveaccessiblename" => "expectAccessibleName",
            "tohaverole" => "expectRole",
            "tobechecked" => "expectChecked",
            "tobeunchecked" => "expectUnchecked",
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
