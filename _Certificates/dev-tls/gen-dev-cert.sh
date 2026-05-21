#!/bin/sh
# Generates a self-signed dev TLS certificate the Kiosk + docker-compose stack
# can use locally. Drops:
#   ca.crt + ca.key       -> Local "PoD Dev CA" trust anchor.
#   server.crt + server.key + server.pfx -> Leaf cert with CN=localhost and SANs
#                                          for localhost, pod.local, 127.0.0.1, ::1.
#
# Set DEV_TLS_PFX_PATH=/app/dev-tls/server.pfx + DEV_TLS_PFX_PASSWORD=devtls in
# `.env` so the docker server binds :443 with this cert. Drop a copy of `ca.crt`
# at `%APPDATA%\LeapPlay\License\ca.crt` so the kiosk's `Grpc.Core` trusts it.
#
# Cert files are gitignored (see top-level .gitignore: _Certificates/**/*.{crt,...}).
# Re-run after expiry. Not for production.

set -e
cd "$(dirname "$0")"

PASSWORD=${DEV_TLS_PFX_PASSWORD:-devtls}

# 1. Local CA
openssl req -x509 -newkey rsa:2048 -nodes -days 365 \
    -config ca-openssl.cnf -extensions v3_ca \
    -keyout ca.key -out ca.crt

# 2. Server key + CSR
openssl req -new -newkey rsa:2048 -nodes \
    -config openssl-localhost.cnf \
    -keyout server.key -out server.csr

# 3. Sign server cert with the CA
openssl x509 -req -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial \
    -out server.crt -days 365 \
    -extfile openssl-localhost.cnf -extensions v3_req

# 4. Bundle into PFX for Kestrel
openssl pkcs12 -export -out server.pfx \
    -inkey server.key -in server.crt -certfile ca.crt \
    -passout pass:"$PASSWORD"

echo
echo "Generated:"
ls -la ca.crt ca.key server.crt server.key server.pfx
echo
echo "Next:"
echo "  cp ca.crt \"\$APPDATA/LeapPlay/License/ca.crt\""
echo "  echo 'DEV_TLS_PFX_PATH=/app/dev-tls/server.pfx' >> ../../.env"
echo "  echo 'DEV_TLS_PFX_PASSWORD=$PASSWORD' >> ../../.env"
