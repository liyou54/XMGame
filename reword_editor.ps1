# GIT_EDITOR: 用 UTF-8 消息覆盖 COMMIT_EDITMSG
Param([string]$msgPath)
$src = "d:\Project\unity\XMGame\msg_utf8.txt"
if (Test-Path $src) { Copy-Item -LiteralPath $src -Destination $msgPath -Force }
