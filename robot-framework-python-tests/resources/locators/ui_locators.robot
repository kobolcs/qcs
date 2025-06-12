*** Variables ***
${LAUNCH_SECTION}    xpath=//section[contains(., 'Latest Launch')]
${LAUNCH_TITLE}      xpath=//h1[contains(text(), 'Latest Launch')]
${IFRAME_FRAME}      xpath=//iframe[@id="launch-details-frame"]
${IFRAME_CONTENT}    xpath=//div[@class="launch-info"]
${LAUNCH_IMG}        xpath=//img[@class="rocket-patch"]
