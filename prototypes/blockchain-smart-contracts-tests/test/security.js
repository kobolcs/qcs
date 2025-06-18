const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("Security tests", function () {
  it("should be vulnerable to reentrancy", async function () {
    const [owner, attacker] = await ethers.getSigners();

    const EtherStore = await ethers.getContractFactory("EtherStore", owner);
    const etherStore = await EtherStore.deploy();
    await etherStore.waitForDeployment();

    // Owner deposits 5 ether
    let tx = await etherStore.deposit({ value: ethers.parseEther("5") });
    let receipt = await tx.wait();
    expect(receipt.gasUsed).to.be.lessThan(150000n);

    expect(await etherStore.getBalance()).to.equal(ethers.parseEther("5"));

    const Attack = await ethers.getContractFactory("AttackEtherStore", attacker);
    const attack = await Attack.deploy(await etherStore.getAddress());
    await attack.waitForDeployment();

    // Attacker drains funds
    await attack.connect(attacker).attack({ value: ethers.parseEther("1") });

    expect(await etherStore.getBalance()).to.equal(0);
  });

  it("should demonstrate uint8 overflow", async function () {
    const Overflow = await ethers.getContractFactory("OverflowExample");
    const overflow = await Overflow.deploy();
    await overflow.waitForDeployment();

    const result = await overflow.add(1);
    expect(result).to.equal(0);
  });
});
