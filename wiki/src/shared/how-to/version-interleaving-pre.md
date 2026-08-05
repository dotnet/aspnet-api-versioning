# Version Interleaving

API versions do not have to be split across different controller classes.  A service author might choose to have a
controller implement multiple API versions simultaneously. Controller actions can subsequently be mapped to specific
API versions. This approach is useful for small version differences but should be used sparingly to prevent developer
confusion and complicate code maintenance. For example: