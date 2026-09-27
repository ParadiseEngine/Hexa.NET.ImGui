import { dotnet } from './_framework/dotnet.js';

const out = document.getElementById('out');
const lines = [];
const exitCode = await dotnet
    .withConfig({ forwardConsole: false })
    .withModuleConfig({ print: line => { lines.push(line); console.log(line); } })
    .run();
out.textContent = lines.join('\n') + `\nexit code ${exitCode}`;
