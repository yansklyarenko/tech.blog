---
Title: "Migrate a blog from OctoPress to Wyam"
Published:
Tags:
    - octopress
    - wyam
    - blogging
    - migration
---

1. Install Wyam (see official docs)
2. Generate a blog with wyam new -r blog
3. Enter basic settings to config.wyam
4. Per each old blog post

    - Remove `layout: post`
    - Capitalize `Title`
    - Replace `Date` with `Published`
    - Remove `comments:*`
    - Replace `Categories` to `Tags`
    - Format tags to one tag per line
    - Add `RedirectFrom` attribute to contain the part of the old URL (after host name)
    - Strip the date from the file name
    - If HTML, rename to MD
    - If HTML, manually re-enter the content, fix links, formatting, etc.
    - If there are images, download an image to /images folder, place the formatted image reference

5. Build with `wyam build`
6. Preview with `wyam preview`
7. Browse to http://localhost:5080 to verify the result
