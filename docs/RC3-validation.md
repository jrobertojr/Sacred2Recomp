# RC3 local validation

Experimental headless mode is opt-in via S2_HEADLESS_TEST=1 and requires UI, audio and input drivers set to none. It was tested on a separate Windows desktop without switching the active desktop. Vulkan initialized on an RTX 3050, loaded 3318180 bytes of native cache and recreated 32 of 32 stored pipelines. The title remained running for the 25-second test and was deliberately terminated. No presenter was active; visual correctness and campaign gameplay are not validated.

The SDK patch adds save flushing, regression tests and opt-in headless title requirements. Apply to the matching ReXGlue SDK source after checking for conflicts. The local build also contains earlier experimental Vulkan modifications not included in this patch.

Local unit test groups passed: content 20 cases / 227 assertions; filesystem 111 / 3322; shader and pacing 22 / 61; geometry and texture 13 / 23489. Groups may overlap. No game assets, extracted executables, generated game code, personal saves or binary caches are published.