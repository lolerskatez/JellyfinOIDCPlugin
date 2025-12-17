// Jellyfin OIDC Global Loader
// This script is loaded globally to inject the OIDC login button on the login page
(function() {
    'use strict';
    
    // Load the actual login script
    const script = document.createElement('script');
    script.src = '/api/oidc/login.js?v=' + Date.now();
    script.type = 'text/javascript';
    script.async = true;
    script.onerror = function() {
        console.warn('[OIDC] Failed to load login script');
    };
    
    // Add to head or body
    (document.head || document.documentElement).appendChild(script);
    
    console.log('[OIDC] Loader injected, waiting for script...');
})();
