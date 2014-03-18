@if (@X)==(@Y) @end /* Harmless hybrid line that begins a JScript comment

::************ Documentation ***********
:::
:::REPL  Search  Replace  [Options  [SourceVar]]
:::REPL  /?
:::
:::  Performs a global search and replace operation on each line of input from
:::  stdin and prints the result to stdout.
:::
:::  Each parameter may be optionally enclosed by double quotes. The double
:::  quotes are not considered part of the argument. The quotes are required
:::  if the parameter contains a batch token delimiter like space, tab, comma,
:::  semicolon. The quotes should also be used if the argument contains a
:::  batch special character like &, |, etc. so that the special character
:::  does not need to be escaped with ^.
:::
:::  If called with a single argument of /? then prints help documentation
:::  to stdout.
:::
:::  Search  - By default, this is a case sensitive JScript (ECMA) regular
:::            expression expressed as a string.
:::
:::            JScript regex syntax documentation is available at
:::            http://msdn.microsoft.com/en-us/library/ae5bf541(v=vs.80).aspx
:::
:::  Replace - By default, this is the string to be used as a replacement for
:::            each found search expression. Full support is provided for
:::            substituion patterns available to the JScript replace method.
:::
:::            For example, $& represents the portion of the source that matched
:::            the entire search pattern, $1 represents the first captured
:::            submatch, $2 the second captured submatch, etc. A $ literal
:::            can be escaped as $$.
:::
:::            An empty replacement string must be represented as "".
:::
:::            Replace substitution pattern syntax is fully documented at
:::            http://msdn.microsoft.com/en-US/library/efy6s3e6(v=vs.80).aspx
:::
:::  Options - An optional string of characters used to alter the behavior
:::            of REPL. The option characters are case insensitive, and may
:::            appear in any order.
:::
:::            I - Makes the search case-insensitive.
:::
:::            L - The Search is treated as a string literal instead of a
:::                regular expression. Also, all $ found in Replace are
:::                treated as $ literals.
:::
:::            B - The Search must match the beginning of a line.
:::                Mostly used with literal searches.
:::
:::            E - The Search must match the end of a line.
:::                Mostly used with literal searches.
:::
:::            V - Search and Replace represent the name of environment
:::                variables that contain the respective values. An undefined
:::                variable is treated as an empty string.
:::
:::            M - Multi-line mode. The entire contents of stdin is read and
:::                processed in one pass instead of line by line. ^ anchors
:::                the beginning of a line and $ anchors the end of a line.
:::
:::            X - Enables extended substitution pattern syntax with support
:::                for the following escape sequences within the Replace string:
:::
:::                \\     -  Backslash
:::                \b     -  Backspace
:::                \f     -  Formfeed
:::                \n     -  Newline
:::                \r     -  Carriage Return
:::                \t     -  Horizontal Tab
:::                \v     -  Vertical Tab
:::                \xnn   -  Ascii (Latin 1) character expressed as 2 hex digits
:::                \unnnn -  Unicode character expressed as 4 hex digits
:::
:::                Escape sequences are supported even when the L option is used.
:::
:::            S - The source is read from an environment variable instead of
:::                from stdin. The name of the source environment variable is
:::                specified in the next argument after the option string.
:::

::************ Batch portion ***********
@echo off
if .%2 equ . (
  if "%~1" equ "/?" (
    findstr "^:::" "%~f0" | cscript //E:JScript //nologo "%~f0" "^:::" ""
    exit /b 0
  ) else (
    call :err "Insufficient arguments"
    exit /b 1
  )
)
echo(%~3|findstr /i "[^SMILEBVX]" >nul && (
  call :err "Invalid option(s)"
  exit /b 1
)
cscript //E:JScript //nologo "%~f0" %*
exit /b 0

:err
>&2 echo ERROR: %~1. Use REPL /? to get help.
exit /b

************* JScript portion **********/
var env=WScript.CreateObject("WScript.Shell").Environment("Process");
var args=WScript.Arguments;
var search=args.Item(0);
var replace=args.Item(1);
var options="g";
if (args.length>2) {
  options+=args.Item(2).toLowerCase();
}
var multi=(options.indexOf("m")>=0);
var srcVar=(options.indexOf("s")>=0);
if (srcVar) {
  options=options.replace(/s/g,"");
}
if (options.indexOf("v")>=0) {
  options=options.replace(/v/g,"");
  search=env(search);
  replace=env(replace);
}
if (options.indexOf("l")>=0) {
  options=options.replace(/l/g,"");
  search=search.replace(/([.^$*+?()[{\\|])/g,"\\$1");
  replace=replace.replace(/\$/g,"$$$$");
}
if (options.indexOf("b")>=0) {
  options=options.replace(/b/g,"");
  search="^"+search
}
if (options.indexOf("e")>=0) {
  options=options.replace(/e/g,"");
  search=search+"$"
}
if (options.indexOf("x")>=0) {
  options=options.replace(/x/g,"");
  replace=replace.replace(/\\\\/g,"\\B");
  replace=replace.replace(/\\b/g,"\b");
  replace=replace.replace(/\\f/g,"\f");
  replace=replace.replace(/\\n/g,"\n");
  replace=replace.replace(/\\r/g,"\r");
  replace=replace.replace(/\\t/g,"\t");
  replace=replace.replace(/\\v/g,"\v");
  replace=replace.replace(/\\x[0-9a-fA-F]{2}|\\u[0-9a-fA-F]{4}/g,
    function($0,$1,$2){
      return String.fromCharCode(parseInt("0x"+$0.substring(2)));
    }
  );
  replace=replace.replace(/\\B/g,"\\");
}
var search=new RegExp(search,options);

if (srcVar) {
  WScript.Stdout.Write(env(args.Item(3)).replace(search,replace));
} else {
  while (!WScript.StdIn.AtEndOfStream) {
    if (multi) {
      WScript.Stdout.Write(WScript.StdIn.ReadAll().replace(search,replace));
    } else {
      WScript.Stdout.WriteLine(WScript.StdIn.ReadLine().replace(search,replace));
    }
  }
}