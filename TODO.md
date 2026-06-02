# TODO - Enable reliable file editing

- [ ] Confirm you want me to edit specific existing file(s) (provide file path(s)) and what behavior/code change you want.
- [ ] Use `read_file` / `search_files` to inspect the exact current code before editing.
- [ ] Propose an edit plan (files + exact code blocks to change).
- [ ] Wait for your explicit approval: “Proceed”.
- [ ] Apply changes using `edit_file` with exact `<<<<<<< SEARCH ... ======= ... >>>>>>> REPLACE` blocks.
- [ ] For new files, apply using `create_file` with the full intended content.
- [ ] Run build/tests/linters (if available) to verify the changes compile.