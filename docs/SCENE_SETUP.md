# Scene wiring reference

This describes dependencies visible in the original scripts. It is not a tested replacement scene.

## Player / camera

Attach NoClipCamera to the first-person camera object. Configure detectLayer for ground / bridge surfaces and obstacleLayer for obstacles. The script uses capsule checks and downward raycasts, and can lock movement / rotation during interactions.

BridgePartCollection uses its own trigger callbacks. Its player object therefore needs an appropriate collider / physics setup in the host scene for those callbacks to fire.

## Component collection

The component names expected by BridgePartCollection are 拱券, 仰天石 and 栏板. Supply trigger colliders, taskTipText, progressText, knowledgePanel, knowledgeText and the noClipCamera reference. Bind the panel's close button to CloseKnowledgePanel.

This checks the original code's names; it does not independently verify the historical accuracy of the coursework's bridge descriptions.

## Dialogue and quiz

StoryController coordinates the three tasks. Assign dialogue panels and text fields, the collection script, quiz panel, three answer buttons and their text labels. Preserve the original scene's NPC interaction setup when using these scripts.

## Repair

Assign a dedicated bridgeFixCamera, fixParts, fixPartLights and ClickRepairDetector. Each repair location needs a suitable collider for raycast detection. Supply the StoryController reference on the click detector and connect any audio/UI references expected by the controller.

The existing scene's Inspector references, NPC colliders, lights, animation and audio configuration are not supplied. Before claiming playability, create and test those bindings in Unity.
