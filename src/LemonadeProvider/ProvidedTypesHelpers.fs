namespace LemonadeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Quotations

// * 'ProvidedTypes.fs' (from the SDK) contains helper types and helper
//   methods for implementing ITypeProvider interface. Its a little ironic
//   that the helpers have a very Object-Oriented feel, when F# is an FP lang
// * Many types in the SDK extend the built-in Reflection types (MemberInfo,
//   PropertyInfo etc)

// TODO: maybe use this one
type InvokeCodeFn = Expr list -> Expr

/// A series of curried helper functions intended to make OO ProvidedTypes interface more functional
[<AutoOpen>]
module internal ProvidedTypesHelpers =

    // root of pipeline
    let inline mkProvidedConstructor (parameters: ProvidedParameter list) (invokeCode: Expr list -> Expr) : ProvidedConstructor =
        ProvidedConstructor(parameters, invokeCode)

    let inline mkReadOnlyProvidedProperty< ^T> (getterCode: Expr list -> Expr) (propName: string) : ProvidedProperty =
         ProvidedProperty(propName, typeof< ^T>, getterCode = getterCode)

    // NOTE: methodName is last because we want to pipe it in
    let inline mkProvidedMethod< ^T> (parameters: ProvidedParameter list) (invokeCode: Expr list -> Expr) (methodName: string) : ProvidedMethod =
        ProvidedMethod(methodName, parameters, typeof< ^T>, invokeCode)

    let inline mkProvidedParameter< ^T> (paramName: string) : ProvidedParameter =
        ProvidedParameter(paramName, typeof< ^T>)

    // NOTE: providedMember is any type (e.g. ProvidedConstructor) that has AddXmlDoc/AddXmlDocDelayed

    // SRTP!!!
    let inline addXmlComment (comment: string) (providedMember: ^a) : ^a =
        (^a : (member AddXmlDoc : string -> unit) (providedMember, comment))
        providedMember

    // SRTP!!!
    let inline addDelayedXmlComment (comment: string) (providedMember: ^a) : ^a =
        (^a : (member AddXmlDocDelayed : (unit -> string) -> unit) (providedMember, (fun () -> comment)))
        providedMember

    // let inline makeTagPropertyWithComment tag comment =
    //   let expr =
    //     fun [tags] ->
    //       <@@ (((%%tags:obj) :?> Dictionary<string, ID3Frame>).[tag]).GetContent() |> unbox @@>
    //   (mkReadOnlyProvidedProperty<string> expr)>> (addDelayedXmlComment comment)
