import { createServer } from "node:http";
import { fixture } from "./fixtures.js";
createServer(async (req, res) => {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
  res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
  if (req.method === "OPTIONS") {
    res.writeHead(204);
    res.end();
    return;
  }
  let raw = "";
  for await (const chunk of req) raw += chunk;
  let data;
  try {
    data = raw ? JSON.parse(raw) : {};
  } catch {
    res.writeHead(400);
    res.end();
    return;
  }
  const result = fixture(req.method, req.url, data);
  res.writeHead(result.status || 200, { "Content-Type": "application/json" });
  res.end(JSON.stringify(result.body));
}).listen(4179, "127.0.0.1", () =>
  console.log(
    "Isolated Circlo test API at 127.0.0.1:4179; synthetic data only.",
  ),
);
