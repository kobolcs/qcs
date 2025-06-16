// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

contract OverflowExample {
    uint8 public maxUint8 = 255;

    function add(uint8 _value) public view returns (uint8) {
        unchecked {
            return maxUint8 + _value;
        }
    }
}
