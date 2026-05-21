del server.key
del server.crt

C:\OpenSSL-Win64\bin\openssl.exe genrsa -out server.key.rsa 2048
C:\OpenSSL-Win64\bin\openssl.exe pkcs8 -topk8 -in server.key.rsa -out server.key -nocrypt
del server.key.rsa

C:\OpenSSL-Win64\bin\openssl.exe req -new -key server.key -out server.csr -config server-openssl.cnf
C:\OpenSSL-Win64\bin\openssl.exe x509 -req -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -extfile server-openssl.cnf -out server.crt -days 365 -sha256 -extensions v3_req
del server.csr
Pause