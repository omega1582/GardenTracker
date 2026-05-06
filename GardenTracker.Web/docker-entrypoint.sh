#!/bin/sh
# Write runtime environment variables into env.js before nginx starts.
# Add any new VITE_* variables here as the app grows.
cat > /usr/share/nginx/html/env.js << EOF
window.__ENV__ = {
  VITE_API_URL: "${VITE_API_URL:-http://localhost:5280}"
};
EOF

exec nginx -g 'daemon off;'
