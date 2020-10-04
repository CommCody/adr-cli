// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Linq
open FSharp.Configuration

open CommandLine
//open CommandLine.Text

type adrStyle =
  | Nyrgard = 0

[<Verb("init", HelpText = "Add file contents to the index.")>]
type InitOptions = {
  [<Value(0, MetaName = "root")>] directory: string
  [<Value(1, MetaName = "root")>] root: string
  [<Option('s', "style", Default = adrStyle.Nyrgard)>] style: adrStyle
  // normal options here
}
[<Verb("add", HelpText = "Clone a repository into a new directory.")>]
type AddOptions = {
  [<Value(0, MetaName = "name")>] name: string
  [<Option('s', "style")>] style: adrStyle option
  [<Option('r', "references", Separator = ',')>]references: int seq
  [<Option('R', "replaces", Separator = ',')>]replaces: int seq
  // normal options here
}
[<Verb("list", HelpText = "Record changes to the repository.")>]
type ListOptions = {
  [<Value(0)>] t: string
  // normal options here
}
//[<Verb("clone", HelpText = "Clone a repository into a new directory.")>]
//type CloneOptions = {
//  // normal options here
//}

type verbs =
  | Init of InitOptions
  | Add of AddOptions
  | List of ListOptions


//let formatLong o =
//  match o with
//    | Some(v) -> string v
//    | _ -> "{None}"

//let formatInput (o : options)  =
//    sprintf "--stringvalue: %s\n-i: %A\n-x: %b\nvalue: %s\n" o.stringValue o.intSequence o.boolValue (formatLong o.longValue)

type parseResult<'t> = 
  | Success of 't
  | Help
  | Version
  | Fail of Error seq

let inline a (result : ParserResult<obj>) =
  match result with
  | :? Parsed<obj> as parsed -> Success parsed.Value
  | :? NotParsed<obj> as notParsed when notParsed.Errors.IsHelp() -> Help
  | :? NotParsed<obj> as notParsed when notParsed.Errors.IsVersion() -> Version
  | :? NotParsed<obj> as notParsed -> Fail(notParsed.Errors)
  | _ -> failwith "invalid parser result"

//let inline (|Success|Help|Version|Fail|) (result : ParserResult<'a>) =
//  match result with
//  | :? Parsed<'a> as parsed -> Success(parsed.Value)
//  | :? NotParsed<'a> as notParsed when notParsed.Errors.IsHelp() -> Help
//  | :? NotParsed<'a> as notParsed when notParsed.Errors.IsVersion() -> Version
//  | :? NotParsed<'a> as notParsed -> Fail(notParsed.Errors)
//  | _ -> failwith "invalid parser result"

type Config = YamlConfig<YamlText = """config:
  directory: 'docs/adr'
  style: 'Nyrgard'""">

//let isAdrDirectory root =
//  let directory = new System.IO.DirectoryInfo(System.IO.Path.Combine(root, ".adr"))
//  directory.Exists

let sanitize string (stripChars: seq<char>) =
  string
  |> Seq.map (fun (c) -> if Seq.contains c stripChars then '-' else c)
  |> String.Concat
 
 
let yymmdd (tw:System.IO.TextWriter) (date:DateTime) = tw.Write("{0:yy.MM.dd}", date)

let createNewAdrFactory style (adrDirectory: System.IO.DirectoryInfo) = 
  match style with
    | adrStyle.Nyrgard ->
      fun number title ->
        let fullTitle = sprintf "%4i-%s.md" number (sanitize title [' '])
        let initialAdr = new System.IO.FileInfo(System.IO.Path.Combine(adrDirectory.FullName, fullTitle))
        use sw = initialAdr.CreateText()
        fprintf sw """# %4i. %s

%a

"## Status

Accepted

## Context

We need to record the architectural decisions made on this project.

## Decision

We will use Architecture Decision Records, as described by Michael Nygard in [this article](http://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions)

## Consequences

See Michael Nygard's article, linked above.""" number title yymmdd DateTime.Today
        0
  
let styleToString style =
  match style with
    | adrStyle.Nyrgard -> "Nyrgard"
    | _ -> "NOSTYLE"

let parseStyleString styleString : adrStyle Option =
  match styleString with
    | "Nyrgard" ->  Some adrStyle.Nyrgard
    | _ -> None

let parseStyleStringWD styleString defaulT: adrStyle =
  match (parseStyleString styleString) with
    | Some style -> style
    | _ -> defaulT


let init (o : InitOptions) =
  let directory = new System.IO.DirectoryInfo(System.IO.Path.Combine(o.root, ".adr"))
  match directory.Exists with
    | true ->
      printfn "The directory \"%s\" is already a adr root directory" o.root
      1
    | false ->
      let _ = directory.Create()
      let configFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, "config"))
      let config = Config()
      config.config.directory <- o.directory
      config.config.style <- (styleToString o.style)
      use sw = configFile.CreateText()
      config.Save(sw)
      let adrDirectory = new System.IO.DirectoryInfo(System.IO.Path.Combine(o.root, o.directory))
      let createNewAdr = createNewAdrFactory o.style adrDirectory
      let _ = adrDirectory.Create()
      createNewAdr 0 "Record Architecture Decisions"

let getConfig directory =
  let config = Config()
  config.Load(System.IO.Path.Combine(directory, ".adr", "config"))
  config.config

let getLatestAdrNumber directory =
  let dirInfo = new System.IO.DirectoryInfo(directory)
  let first = (Seq.sortByDescending (fun (f:System.IO.FileInfo) -> (f.Name)) (dirInfo.EnumerateFiles("*.md"))).FirstOrDefault()
  match first with
  | null -> 0
  | _ -> first.Name.Split('-', 1, StringSplitOptions.RemoveEmptyEntries).[0] |> int

let add (o : AddOptions) =
  let config = getConfig "."
  let createNewAdr =
    createNewAdrFactory (parseStyleStringWD config.style adrStyle.Nyrgard) (new System.IO.DirectoryInfo(config.directory))
  let latestAdr = getLatestAdrNumber config.directory
  createNewAdr (latestAdr+1) o.name
  //0

let list (o : ListOptions) =
  0

type BTokenError(token) =
  inherit TokenError(ErrorType.BadVerbSelectedError, token)

let parse (args : string[]) =
  let result = Parser.Default.ParseArguments<InitOptions, AddOptions, ListOptions>(args)
  match (a result) with
    | Success(verb) -> 
        match verb with
        | :? InitOptions as opts -> Success(Init opts)
        | :? AddOptions as opts -> Success(Add opts)
        | :? ListOptions as opts -> Success(List opts)
        | _ -> Fail([BTokenError "B"])
    | Fail(errors) -> Fail(errors)
    | Help -> Help
    | Version -> Version

let handle verb =
  match verb with
  | Init(options) -> init options
  | Add(options) -> add options
  | List(options) -> list options

[<EntryPoint>]
let main args =
  let result = parse args
  match result with
    | Success(verbs) -> handle verbs
    | Fail(errs) ->
      printfn "Invalid: %A, Errors: %i" args (Seq.length errs)
      0
    | Help | Version -> (0)
