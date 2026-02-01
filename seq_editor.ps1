# GIT_SEQUENCE_EDITOR: 将 8552a7e6 的 pick 改为 reword
Param([string]$todoPath)
(Get-Content $todoPath -Raw) -replace '^pick 8552a7e6 ', 'reword 8552a7e6 ' | Set-Content $todoPath -NoNewline
