# custom-umbraco-health-checks
Custom health checks for Umbraco websites.
Designed to be used with error messages pulled from ~/Config/Lang/en-US.user.xml

1. Custom Errors Mode Health Check
    - Ensures that Web.config and umbracoSettings.config are configured so that custom 404 and 500 error pages are shown, rather than Umbraco's default error pages.

2. Document Type Icon Health Check
    - Checks that all document types within the project have been given an icon.

3. Empty Recycle Bin Health Check
    - Checks that the recycle bin is empty.
    
4. Error 404 Response Health Check
    - Checks that a custom page is returned in the event of a 404 error, rather than the default Umbraco error page.
    
5. Favicon Health Check
    - Checks whether the site has a favicon.

6. HTML Language Attribute Health Check
    - Checks that the <html> tag has the language attribute set.
    
7. Image Alt Tag Property Health Check
    - Checks that the image media type has a property through which alt tag text can be added to images.
    
8. Media Root Health Check
    - Checks that all media items are inside folders, and not in the root.
    
9. ReCatpcha Key Health Check
    - Checks that a Google ReCaptcha key has been added to Web.config (ignore if the site does either has no contact form, or does not need ReCaptcha protection).
    
10. Redirect URL Management Health Check
    - Checks that all unused links are removed from redirect URL management.

11. Robots.txt Health Check
    - Check for the existence of a robots.txt file.
    
12. Server Error Page Health Check
    - Check for the existence of a 500.html file for use in the case of internal server errors.
    
13. Site Map Health Check
    - Checks that an XML site map exists at '/sitemap'.
    
14. User Groups Health Check
    - Checks that all required user groups have been created.
