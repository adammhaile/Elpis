%%{

machine uri;

scheme = alpha (alpha | digit | "+" | "-" | ".")+;

escaped = "%" xdigit xdigit;
mark = "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")";
reserved = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ",";
unreserved = alpha | digit | mark;
pchar = unreserved | escaped | ":" | "@" | "&" | "=" | "+" | "$" | ",";
uric = reserved | unreserved | escaped;
uric_no_slash = unreserved | escaped | ";" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ",";
rel_segment = (unreserved | escaped | ";" | "@" | "&" | "=" | "+" | "$" | ",")+;
reg_name = (unreserved | escaped | "$" | "," | ";" | ":" | "@" | "&" | "=" | "+" )+;
userinfo = (unreserved | escaped | ";" | ":" | "&" | "=" | "+" | "$" | "," )*;

param = pchar*;
segment = pchar* (";" param)*;
path_segments = segment ("/" segment)*;
uri_abs_path = "/" path_segments;

rel_path = rel_segment uri_abs_path?;

uri_query = uric*;
uri_fragment = uric*;

ipv4address = digit{1,3} "." digit{1,3} "." digit{1,3} "." digit{1,3};
toplabel = alpha | alpha (alnum | "-")* alnum;
domainlabel = alnum | (alnum (alnum | "-")* alnum);
hostname = (domainlabel ".") toplabel "."?;

# might be better to condense host to (alnum | "." | "-")*;
host = hostname | ipv4address;
port = digit*;

hostport = host (":" port)?;
server = ((userinfo "@")? hostport)?;
uri_authority = server | reg_name;
net_path = "//" uri_authority uri_abs_path?;

opaque_part = uric_no_slash uric*;
hier_part = (net_path | uri_abs_path) ("?" uri_query)?;


uri_absolute_uri = scheme ":" (hier_part | opaque_part);
relative_uri = (uri_abs_path | rel_path) ("?" uri_query)?;

uri_reference = (uri_absolute_uri | relative_uri)? ("#" uri_fragment)?;

}%%
