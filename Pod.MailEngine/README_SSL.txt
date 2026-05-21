https://github.com/dotnet/corefx/blob/master/Documentation/architecture/cross-platform-cryptography.md
https://github.com/dotnet/corefx/issues/32875
Where does .net core search for certificates on linux platform?
OpenSSL has defaults for the dir and file. You can overwrite these defaults by specifying envvars (SSL_CERT_DIR/SSL_CERT_FILE).
For example, you create a certificate bundle file and set SSL_CERT_FILE to it.
Then you launch your dotnet app, and it will use the bundle certificates (+ the default dir certificates).