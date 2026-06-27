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
            "expectnotattached" => "expectDetached",
            "expectnotdetached" => "expectAttached",
            "expectnoteditable" => "expectNotEditable",
            "expectnotempty" => "expectNotEmpty",
            "expectnotfocused" => "expectNotFocused",
            "expectnotinviewport" => "expectNotInViewport",
            "expectvalue" => "expectValue",
            "expectvalues" => "expectValues",
            "expectattribute" => "expectAttribute",
            "expectaccessiblename" => "expectAccessibleName",
            "expectrole" => "expectRole",
            "expectchecked" => "expectChecked",
            "expectnotchecked" => "expectUnchecked",
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
            "tobenotattached" => "expectDetached",
            "tobenotdetached" => "expectAttached",
            "tobenoteditable" => "expectNotEditable",
            "tobenotempty" => "expectNotEmpty",
            "tobenotfocused" => "expectNotFocused",
            "tobenotinviewport" => "expectNotInViewport",
            "tohavevalue" => "expectValue",
            "tohavevalues" => "expectValues",
            "tohaveattribute" => "expectAttribute",
            "tohaveaccessiblename" => "expectAccessibleName",
            "tohaverole" => "expectRole",
            "tobechecked" => "expectChecked",
            "tobenotchecked" => "expectUnchecked",
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
